import moment from "moment";

const DISPLAY_DATE_FORMAT = "DD/MM/YYYY";
const ISO_DATE_FORMAT = "YYYY-MM-DD";
const API_DATE_SUFFIX = "T00:00:00";

function getIsoDate(value?: string | null) {
  if (!value) {
    return null;
  }

  return value.match(/^\d{4}-\d{2}-\d{2}/)?.[0] ?? null;
}

export const dateUtils = {
  toDisplay(value?: string | null) {
    if (!value) {
      return "";
    }

    if (moment(value, DISPLAY_DATE_FORMAT, true).isValid()) {
      return value;
    }

    const isoDate = getIsoDate(value);

    if (!isoDate) {
      return value;
    }

    return moment(isoDate, ISO_DATE_FORMAT, true).format(DISPLAY_DATE_FORMAT);
  },
  toApi(value?: string | null) {
    if (!value) {
      return "";
    }

    const displayDate = moment(value, DISPLAY_DATE_FORMAT, true);

    if (displayDate.isValid()) {
      return `${displayDate.format(ISO_DATE_FORMAT)}${API_DATE_SUFFIX}`;
    }

    const isoDate = getIsoDate(value);

    if (isoDate) {
      return `${isoDate}${API_DATE_SUFFIX}`;
    }

    return value;
  },
  isDisplayDate(value?: string | null) {
    if (!value) {
      return false;
    }

    return moment(value, DISPLAY_DATE_FORMAT, true).isValid();
  },
  formatDateTime(value?: string | null) {
    if (!value) return "—";
    const utcDate = value.endsWith("Z") ? value : value + "Z";
    return new Date(utcDate).toLocaleString("pt-BR", { dateStyle: "short", timeStyle: "short" });
  },
  formatDate(value?: string | null) {
    if (!value) return "—";
    const utcDate = value.endsWith("Z") ? value : value + "Z";
    return new Date(utcDate).toLocaleDateString("pt-BR");
  },
};
