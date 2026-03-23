import Select, { Props as ReactSelectProps } from "react-select";
import AsyncSelect, { AsyncProps } from "react-select/async";
import CreatableSelect, { CreatableProps } from "react-select/creatable";
import AsyncCreatableSelect, {
  AsyncCreatableProps,
} from "react-select/async-creatable";

import {
  Col,
  Form,
  FormControlProps,
} from "react-bootstrap";
import styled from "styled-components";
import { TextFormFieldType } from "@/components/form/TextFormField/TextFormFieldType";

import { Fragment, Suspense, useEffect, useState } from "react";
import {
  AiOutlineEye,
  AiOutlineEyeInvisible,
} from "react-icons/ai";

import { Option } from "@/types/Option";

import DatePicker, {
  ReactDatePickerProps as DatePickerBaseProps,
} from "react-datepicker";
import ptBR from "date-fns/locale/pt-BR";
import { MdCloudUpload } from "react-icons/md";
import { useSuspenseQuery } from "@tanstack/react-query";
import { format } from "@/helpers/format";
import moment from "moment";
import CustomTooltip from "@/components/Tooltip";

interface DatePickerComponentProps
  extends Omit<DatePickerBaseProps, "onChange"> {
  componentType: string;
  onChange?: (date: Date | null) => void;
}

export type TextFormFieldProps<T> =
  | InputProps<T>
  | SelectProps<T>
  | CreatableSelectProps<T>
  | ImageProps<T>
  | AsyncSelectProps<T>
  | AsyncCreatableSelectProps<T>
  | DatePickerProps<T>
  | CheckboxProps<T>
  | SelectGroupProps<T>;

export const TextFormField = <T extends any>({
  label,
  required,
  col,
  tooltip,
  ...props
}: TextFormFieldProps<T>) => {

  return (
    <FormGroup
      as={Col}
      {...(col ? { md: col } : {})}
      forwardedAs={Form.Group}
      className={props.className}
      name={props.name}
    >
      {label && (
        <Form.Label className="text-form-field-label">
          {label} {required ? <span style={{ color: "red" }}>*</span> : null} {tooltip && <CustomTooltip text={tooltip} style={{ padding: 0 }} />}
        </Form.Label>
      )}
      {props.componentType === TextFormFieldType.CHECKBOX ? (
        <CheckboxComponent {...props} />
      ) : props.componentType === TextFormFieldType.INPUT ? (
        <InputComponent {...props} />
      ) : props.componentType === TextFormFieldType.SELECT ? (
        <SelectComponent {...props} />
      ) : props.componentType === TextFormFieldType.SELECT_CREATABLE ? (
        <SelectCreatableComponent {...props} />
      ) : props.componentType === TextFormFieldType.SELECT_ASYNC ? (
        <AsyncSelectComponent {...props} />
      ) : props.componentType === TextFormFieldType.SELECT_ASYNC_CREATABLE ? (
        <AsyncCreatableSelectComponent {...props} />
      ) : props.componentType === TextFormFieldType.IMAGE ? (
        <ImageComponent {...props} />
      ) : props.componentType === TextFormFieldType.DATE_PICKER ? (
        <DatePickerComponent {...props} />
      ) : props.componentType === TextFormFieldType.SELECT_GROUP ? (
        <SelectGroupComponent {...props} />
      ) : null}
      {props.componentType === TextFormFieldType.INPUT ? (
        <Form.Control.Feedback type="invalid">
          {props.formikError}
        </Form.Control.Feedback>
      ) : (
        <span className="invalid">{props.formikError}</span>
      )}
    </FormGroup>
  );
}

type BaseFormFieldProps<T = any> = {
  gridName?: string;
  label?: string;
  formikError?: string;
  name: keyof T;
  required?: boolean;
  debounce?: boolean;
  debounceTime?: number;
  className?: string;
  renderIf?: boolean;
  disabled?: boolean;
  defaultValue?: any;
  handleChange?: (value: any) => void;
  handleBlur?: (e: any) => void;
  col?: number
  tooltip?: string
};

type CheckboxProps<T = any> = Omit<FormControlProps, "value"> & BaseFormFieldProps<T> & { componentType: TextFormFieldType.CHECKBOX, value: boolean, switch?: boolean }
type InputProps<T = any> = FormControlProps & BaseFormFieldProps<T> & { componentType: TextFormFieldType.INPUT, format?: (value: any) => any, money?: boolean, password?: boolean, textarea?: boolean, height?: number, mask?: string, disableAutoComplete?: boolean };
type SelectProps<T = any> = ReactSelectProps & BaseFormFieldProps<T> & { componentType: TextFormFieldType.SELECT };
type AsyncSelectProps<T = any> = AsyncProps<any, any, any> & BaseFormFieldProps<T> & { componentType: TextFormFieldType.SELECT_ASYNC, asyncLoad: (inputValue: string, signal?: AbortSignal) => Promise<Option[]>, fieldId?: string, date?: Date };
type AsyncCreatableSelectProps<T = any> = AsyncCreatableProps<any, any, any> & BaseFormFieldProps<T> & { componentType: TextFormFieldType.SELECT_ASYNC_CREATABLE, asyncLoad: (inputValue?: string, signal?: AbortSignal) => Promise<Option[]>, fieldId?: string };
type CreatableSelectProps<T = any> = CreatableProps<any, any, any> & BaseFormFieldProps<T> & { componentType: TextFormFieldType.SELECT_CREATABLE };
type DatePickerProps<T = any> = DatePickerComponentProps & BaseFormFieldProps<T> & { componentType: TextFormFieldType.DATE_PICKER, value?: string, selectRange?: boolean, icon?: any, placeholder?: string };
type ImageProps<T = any> = BaseFormFieldProps<T> & { componentType: TextFormFieldType.IMAGE, multiple?: boolean, accept?: HTMLInputElement["accept"], disabled?: boolean, value?: File | string };
type SelectGroupProps<T = any> = ReactSelectProps & BaseFormFieldProps & { componentType: TextFormFieldType.SELECT_GROUP, options: { label: string, identifier: string, options: Option[] }[] }

const CheckboxComponent = ({
  componentType,
  name: fieldName,
  ...props
}: CheckboxProps) => {
  const handleChange = (event: { currentTarget: { checked: boolean } }) => {
    if (props.handleChange) {
      props.handleChange({
        target: { name: fieldName, value: event.currentTarget.checked },
      });
    }
  };

  return (
    <>
      <Form.Check
        name={fieldName.toString()}
        onChange={handleChange}
        label={props.placeholder}
        disabled={props.disabled}
        checked={!!props.value}
        type={props.switch ? "switch" : "checkbox"}
      />
    </>
  );
};
const AsyncSelectComponent = ({
  componentType,
  ...props
}: AsyncSelectProps) => {
  const { data: options } = useSuspenseQuery<Option[]>({
    queryKey: ["async-select-component", props.name, props.id],
    meta: { fetchFn: () => props.asyncLoad("") }
  });

  const [value, setValue] = useState(props.value)

  useEffect(() => changeValue(props.value), [props.value])

  function changeValue(newValue: any) {
    setValue(((props.isMulti ?? false)
      ? options?.filter(option => newValue?.some((x: string) => x === (option as any)[props.fieldId ?? "id"])) ?? []
      : options?.find(option => (option as any)[props.fieldId ?? "id"] === newValue)
    ) ?? [])
  }

  return (
    <>
      <Suspense>
        <AsyncSelect
          classNamePrefix="select2"
          cacheOptions
          defaultOptions
          loadOptions={(inputValue, callback) => {
            if (!options) {
              callback([]);
              return;
            }

            callback(options?.filter(item => `${item.name}`.toLowerCase().includes(inputValue?.toLowerCase())));
            return;
          }}
          isDisabled={props.disabled ?? props.isDisabled}
          placeholder={props.placeholder ?? "Selecione..."}
          isClearable={props.isClearable}
          loadingMessage={() => "Carregando..."}
          isOptionDisabled={option => !((option as any)?.active ?? true)}
          className={`select2 ${!!props.formikError && "select2__error"}`}
          isMulti={props.isMulti ?? false}
          value={value}
          closeMenuOnSelect={!props.isMulti}
          {...(props.getOptionValue ? { getOptionValue: props.getOptionValue } : { getOptionValue: option => (option as any)[props.fieldId ?? "id"] })}
          {...(props.getOptionLabel ? { getOptionLabel: props.getOptionLabel } : { getOptionLabel: option => (option as any).name })}
          noOptionsMessage={() => "Nenhum registro encontrado"}
          onChange={newValue => {
            let event: any = { target: { name: props.name, value: undefined } }
            if (props.isMulti && Array.isArray(newValue)) {
              event.target.value = newValue.map(option => option[props.fieldId ?? "id"])
            }
            else if (newValue === null) {
              event.target.value = null
            }
            else {
              event.target.value = (newValue as any)[props.fieldId ?? "id"]
            }

            if (props.handleChange)
              props.handleChange(event);

            changeValue(event.target.value)
          }}
        />
      </Suspense>
    </>
  );
};
const AsyncCreatableSelectComponent = ({
  componentType,
  ...props
}: AsyncCreatableSelectProps) => {
  const { data: options } = useSuspenseQuery<Option[]>({
    queryKey: ["async-creatable-select-componente", props.name],
    meta: { fetchFn: () => props.asyncLoad("") }
  });

  const [optionsSelected, setOptionsSelect] = useState<Option[] | Option | undefined>();

  useEffect(() => {
    if (props.value) {
      setOptionsSelect(() => {
        if (Array.isArray(props.value)) {
          return options?.filter((option) =>
            props.value.some((x: string) => x === option.id)
          );
        } else {
          return options?.find((option) => option.id === props.value);
        }
      });
    }
  }, [options]);

  useEffect(() => {
    if (props.value) {
      setOptionsSelect(prev => {
        if (Array.isArray(prev)) {
          const selected = options?.filter(option => props.value.some((x: string) => x === option.id)) || []
          const allSelected = [...prev, ...selected]

          return allSelected.filter((item, index) => allSelected.indexOf(item) === index)
        } else if (Array.isArray(props.value)) {
          return options?.filter(option => props.value.some((x: string) => x === option.id))
        }
        else {
          return options?.find(option => option.id === props.value)
        }
      })
    }
  }, [props.value])

  return <AsyncCreatableSelect
    classNamePrefix="select2"
    cacheOptions
    defaultOptions
    closeMenuOnSelect={!props.isMulti}
    loadOptions={(inputValue, callback) => {
      if (!options) {
        callback([])
        return;
      }

      callback(options?.filter(item => `${item.name}`.toLowerCase().includes(inputValue?.toLowerCase())));
      return;
    }}
    value={optionsSelected ?? null}
    isDisabled={props.disabled ?? props.isDisabled}
    isMulti={props.isMulti}
    className={`select2 ${!!props.formikError && "select2__error"}`}
    placeholder={props.placeholder}
    getOptionValue={(option: any) => option?.id}
    isOptionDisabled={option => !((option as any)?.active ?? true)}
    getOptionLabel={(option: any) => option?.name}
    getNewOptionData={(inputValue, optionLabel) => ({ id: inputValue, name: optionLabel }) as Option}
    loadingMessage={() => "Carregando..."}
    noOptionsMessage={() => "Nenhum registro encontrado"}
    onChange={newValue => {
      let event: { target: { name: string, value?: any } } = { target: { name: props.name } }
      if (Array.isArray(newValue)) event.target.value = newValue.map(option => option.id)
      else event.target.value = (newValue as any).id

      setOptionsSelect(newValue as Option | Option[] | undefined)
      if (props.handleChange) props.handleChange(event)
    }}
  />
}
const SelectCreatableComponent = ({ componentType, ...props }: CreatableSelectProps) => {
  return (
    <CreatableSelect
      classNamePrefix="select2"
      noOptionsMessage={() => "Nenhum registro encontrado"}
      value={props.options?.find(option => option.id === props.value) ?? []}
      closeMenuOnSelect={props.isMulti}
      isMulti={props.isMulti}
      isDisabled={props.disabled ?? props.isDisabled}
      isOptionDisabled={option => !((option as any)?.active ?? true)}
      className={`select2 ${!!props.formikError && "select2__error"}`}
      getOptionValue={option => option.id}
      getOptionLabel={option => option.name}
      getNewOptionData={(inputValue, optionLabel) => ({ id: inputValue, name: optionLabel })}
      onChange={newValue => {
        let event = { target: { name: props.name, id: (newValue as any).id } }
        if (props.handleChange) props.handleChange(event)
      }}
    />
  )
}
const SelectComponent = ({ componentType, ...props }: SelectProps) => {
  return (
    <Select
      name={props.name}
      options={props.options}
      classNamePrefix="select2"
      isDisabled={props.disabled ?? props.isDisabled}
      className={`select2 ${!!props.formikError && "select2__error"}`}
      placeholder={props.placeholder ?? "Selecione..."}
      getOptionValue={option => props.getOptionValue ? props.getOptionValue(option) : (option as any).id}
      getOptionLabel={option => props.getOptionLabel ? props.getOptionLabel(option) : (option as any).name}
      isOptionDisabled={(option, selectValue) => !!props.isOptionDisabled ? props.isOptionDisabled(option, selectValue) : !((option as any)?.active ?? true)}
      closeMenuOnSelect={props.isMulti}
      filterOption={props.filterOption}
      defaultValue={props.options?.find(option => (option as any).id == props.defaultValue)}
      {...(props.value ? {
        value: props.options?.find((item: any) => (item as any).id == props.value)
      } : {})}
      isMulti={props.isMulti}
      isClearable={props.isClearable}
      noOptionsMessage={() => "Nenhum registro encontrado"}
      onChange={newValue => {
        let event = { target: { name: props.name, value: (newValue as any)?.id } }
        if (props.isMulti) {
          event.target.value = (newValue as any).map((item: any) => item?.id)
        }
        if (props.handleChange) props.handleChange(event)
      }}
    />
  )
}
const InputComponent = ({ componentType, formikError, name: fieldName, ...props }: InputProps) => {
  const handleChange = (event: { target: { name: string, value: any } }) => {
    if (props.handleChange) {
      if (props.mask) {
        event.target.value = event.target.value.replace(/[^0-9]/g, "");
        const maskers = (props.mask ?? "").replace(/[#]/g, "").split("");
        event.target.value = format.toMask(event.target.value, props.mask);

        if (event.target.value?.length > 0 && maskers.includes(event.target.value.at(-1) ?? "")) {
          const numbers = event.target.value.match(/\d/g);
          if (numbers) {
            event.target.value = event.target.value.substring(0, event.target.value.lastIndexOf(numbers[numbers.length - 1]) + 1)
          }
          else {
            event.target.value = event.target.value.slice(0, -1);
          }
        }
      }

      props.handleChange(event)
    }
  }

  const inputValue = props.format
    ? props.format(props.value)
    : props.mask && typeof props.value === "string"
      ? format.toMask(props.value, props.mask)
      : props.value ?? "";

  const [showPassword, setShowPassword] = useState<boolean>(false);
  if (props.password) {
    return (
      <Fragment>
        <Form.Control
          placeholder={props.placeholder}
          name={fieldName.toString()}
          isInvalid={!!formikError}
          onChange={handleChange}
          onBlur={props.onBlur}
          onFocus={props.onFocus}
          disabled={props.disabled}
          value={inputValue}
          type={showPassword ? "text" : "password"}
          className={`password-input`}
          autoComplete={props.disableAutoComplete ? "off" : "on"}
        />
        <button
          type="button"
          onClick={() => setShowPassword(!showPassword)}
          style={{
            backgroundColor: "transparent",
            position: "absolute",
            border: "none",
            right: "7px",
            top: "36px",
            fontSize: "15px",
          }}
          className={`password-icon ${!!formikError && "is-invalid"}`}
        >
          {showPassword ? <AiOutlineEye /> : <AiOutlineEyeInvisible />}
        </button>
      </Fragment>
    )
  }
  else {
    return (
      <>
        <Form.Control
          placeholder={props.placeholder}
          name={fieldName.toString()}
          isInvalid={!!formikError}
          onChange={handleChange}
          onBlur={props.onBlur}
          onFocus={props.onFocus}
          disabled={props.disabled}
          autoComplete={props.disableAutoComplete ? "off" : "on"}
          {...(props.textarea ? { as: "textarea" } : {})}
          value={inputValue}
          style={{ width: "100%", height: '100% !important', ...props.style }}
        />
      </>
    )
  }
}
const SelectGroupComponent = ({ componentType, ...props }: SelectGroupProps) => {
  function getValue() {
    if (!!props.value) {
      const [identifier, _] = (props.value as string)?.split('|');
      const group = props.options?.find((item: any) => item.identifier === identifier);
      return (group as any)?.options?.find((item: any) => item.id === props.value);
    }

    return null;
  }

  return (
    <Select
      name={props.name}
      options={props.options}
      classNamePrefix="select2"
      isDisabled={props.disabled ?? props.isDisabled}
      formatGroupLabel={formatGroupLabel}
      className={`select2 ${!!props.formikError && "select2__error"}`}
      placeholder={props.placeholder ?? "Selecione..."}
      getOptionValue={option => (option as any).id}
      getOptionLabel={option => (option as any).name}
      isOptionDisabled={(option, selectValue) => !!props.isOptionDisabled ? props.isOptionDisabled(option, selectValue) : !((option as any)?.active ?? true)}
      closeMenuOnSelect={props.isMulti}
      filterOption={props.filterOption}
      value={getValue()}
      isMulti={props.isMulti}
      isClearable={props.isClearable}
      noOptionsMessage={() => "Nenhum registro encontrado"}
      onChange={newValue => {
        let event = { target: { name: props.name, value: (newValue as any)?.id } }
        if (props.isMulti) {
          event.target.value = (newValue as any).map((item: any) => item?.id)
        }
        if (props.handleChange) props.handleChange(event)
      }}
    />
  )
}
const ImageComponent = ({ componentType, ...props }: ImageProps) => {
  const [file, setFile] = useState<File | string | undefined>(props.value);

  function getFileNameByUrl(url: string) {
    const [uri, _] = url.split("?");
    return uri.split("/").slice(-1);
  }

  return (
    <div style={{ display: "flex", gap: "10px" }}>
      <FileInput>
        <label>
          <MdCloudUpload />
          Escolha um arquivo
          <input
            type="file"
            disabled={props.disabled}
            multiple={props.multiple ?? false}
            accept={props.accept ?? "*/*"}
            onChange={(event) => {
              const file = event.target?.files?.[0];
              setFile(file);
              if (props.handleChange) {
                props.handleChange?.({
                  target: {
                    name: props.name,
                    value: file,
                  },
                });
              }
            }}
          />
        </label>
      </FileInput>
      {file instanceof File && file.type === "application/pdf" ? (
        <a href={URL.createObjectURL(file)} target="_blank">
          {" "}
          {file.name}{" "}
        </a>
      ) : (
        file instanceof File && (
          <img
            src={URL.createObjectURL(file)}
            alt="File"
            style={{ maxWidth: "50px", maxHeight: "50px" }}
          />
        )
      )}
      {typeof file === "string" && ![".png", ".jpg", ".jpeg"].includes(file) ? (
        <a href={file} target="_blank">
          {" "}
          {getFileNameByUrl(file)}{" "}
        </a>
      ) : (
        typeof file === "string" && (
          <img
            src={file}
            alt="File"
            style={{ maxWidth: "50px", maxHeight: "50px" }}
          />
        )
      )}
    </div>
  );
};

const DatePickerComponent = ({ componentType, ...props }: DatePickerProps) => {
  return (
    <DatePicker
      name={props.name}
      disabled={props.disabled}
      showYearDropdown={props.showYearDropdown ?? false}
      yearDropdownItemNumber={props.yearDropdownItemNumber ?? undefined}
      placeholderText={props.placeholderText}
      scrollableYearDropdown
      className={`form-control ${props.calendarClassName} ${!!props.formikError && "is-invalid"}`}
      wrapperClassName="datePicker"
      dateFormat={props.dateFormat ?? "DD/MM/YYYY"}
      showMonthYearPicker={props.showMonthYearPicker ?? false}
      showYearPicker={props.showYearPicker ?? false}
      selectsRange={props.selectRange ?? false}
      value={typeof (props.value ?? props.defaultValue) === "string" ? props.value ?? props.defaultValue : props.value ?? props.defaultValue ?? null}
      onChange={(value) => {
        let event = {
          target: {
            name: props.name,
            value: Array.isArray(value)
              ? [
                value[0] ? moment(value[0]).format("DD/MM/YYYY") : null,
                value[1] ? moment(value[1]).format("DD/MM/YYYY") : null,
              ]
              : props.showMonthYearPicker ?
                moment(value).format("MM/YYYY")
                : value
                  ? moment(value ?? 0).format("DD/MM/YYYY")
                  : null,
          },
        };
        if (props.handleChange)
          props.handleChange?.(event);
      }}
      isClearable={props.isClearable ?? false}
      locale={ptBR}
      calendarClassName="calendar-styles"
      clearButtonClassName={`datepicker-clear-btn`}
    />
  );
};

const formatGroupLabel = (data: any) => (<div style={{ fontSize: 12, backgroundColor: "#efefef", padding: 0, margin: 0 }}><span><i>{data.label}</i></span></div>);

const FormGroup = styled(Form.Group)`
  position: relative;
  height: 100%;
  
  grid-area: ${(props) => props.name};

  label {
    font-size: 13px;
  }

  .form-control {
    padding: 8px 10px;
    border-color: var(--bs-border-color);
    height: 38px !important;
    
    &:focus {
      color: #495057;
      background-color: #fff;
      border-color: #e8a092;
      outline: 0;
      box-shadow: 0 0 0 0.25rem rgba(209, 65, 37, 0.25);
    }
  }

  .react-datepicker-popper,
  .react-datepicker-popper > div > div:first-child,
  .react-datepicker__month-container,
  .react-datepicker__month,
  .react-datepicker__monthPicker {
    min-width: 350px;
    min-height: 220px;
  }

  .react-datepicker__header {
    background-color: var(--light-color);

    .react-datepicker__day-names {
      padding-top: 5px;
      display: flex;
      justify-content: space-around;
      width: 100%;
    }
  }

  .react-datepicker-wrapper {
    width: 100%;
    cursor: pointer;
  }

  .calendar-input {
    cursor: default;
    position: relative;
    border-radius: var(--bs-border-radius);

    &:hover {
      cursor: pointer;
    }
  }

  .react-datepicker__navigation {
    border-radius: 50%;
    background-color: var(--mbu-title-bakground);
    top: 7px;
    width: 22px;
    height: 22px;

        .react-datepicker__navigation-icon {
            margin-top: 10px;
        }
        .react-datepicker__navigation-icon--next {
            margin-left: 6px;
        }
        .react-datepicker__navigation-icon--previous {
            margin-right: 6px;
        }
    }
    .react-datepicker__navigation--previous {
        left: 18px;
    }
    .react-datepicker__navigation--next {
        right: 18px;
    }

    .datepicker-clear-btn::after {
        z-index: 0 !important;
    }


    .input-group-text {
        position: absolute;
        right: 5px;
        border: none;
        bottom: 10%;
        top: 10%;
        z-index: 0;
        background: none;

    svg {
      font-size: 16px;
    }
  }

  /* .datepicker-clear-btn {
    &:after {
      position: absolute;
      right: 10px;
      border: none;
      bottom: 25%;
      top: 25%;
      z-index: 100;
      font-weight: 500;
      font-size: 15px;
      padding: 1px;
    }
    &:hover {
      background: transparent;
    }
  } */

  .calendar-styles {
    width: 100%;
    min-width: 200px;

    .react-datepicker__header,
    .react-datepicker__current-month {
      font-size: 14px;
    }
    .react-datepicker__day-name {
      width: fit-content;
      margin: 0 3px;
    }

    .react-datepicker__month-container {
      width: 100%;
    }
    .react-datepicker__week,
    .react-datepicker__month-wrapper {
      display: flex;
      justify-content: space-around;
      align-items: center;
    }
    .react-datepicker__day {
      font-size: 14px;
    }
    .react-datepicker__day--outside-month {
      opacity: 0.7;
    }
    .react-datepicker__month {
      display: flex;
      flex-direction: column;
      font-size: 14px;
      justify-content: space-around;
    }

    .react-datepicker__month-text,
    .react-datepicker__day {
      display: flex;
      justify-content: center;
      align-items: center;
      font-weight: 500;
      padding: 5px 20px;
    }
    .react-datepicker__month-text {
      padding: 20px 55px;
    }
  }

  input,
  select,
  .select2__single-value,
  .select2__value-container {
    font-size: 13px;

    &:focus {
      box-shadow: none;
      border-color: var(--primary);
    }
  }

  .select2 {
    font-size: 12px;
    white-space: nowrap;

    &__error {
      .select2__control {
        border-color: var(--red);
      }
    }

    .select2__control {
      &.select2__control--is-focused {
        border: 1px solid var(--primary);
        outline: none;
      }
    }
  }

  .invalid {
    width: 100%;
    margin-top: 0.25rem;
    font-size: 80%;
    color: var(--bs-form-invalid-color);
  }

  .currency-info {
    position: absolute;
    top: 0;
    margin: auto;
    right: 10px;

    svg {
      font-size: 16px;
    }
  }
  .password-icon {
    color: var(--bs-body-color);
    top: 36px;
  }

  .password-icon.is-invalid {
    color: var(--bs-form-invalid-border-color);
  }

  .password-input.is-invalid {
    background-image: none;
  }
`;

const FileInput = styled.div`
  border: 1px solid var(--primary);
  padding: 15px 22px;
  border-radius: 4px;
  transition: all 0.2s ease-in-out;
  width: fit-content;

  &:hover {
    cursor: pointer;
    background-color: var(--primary);
    color: var(--text-color-inverted);
  }

  svg {
    margin-right: 10px;
    font-size: 16px;
  }

  input {
    display: none;
  }
`;
