import { createContext, ReactNode, useContext, useState } from 'react';
import { Button, Modal } from 'react-bootstrap';
import { AlertTriangle, CheckCircle, Info, XCircle, Icon as FeatherIcon } from 'react-feather';
import { errorHandling } from '@/utils/errorHandling';

type DialogVariant = 'danger' | 'success' | 'warning' | 'info';

interface DialogAction {
    label: string;
    variant?: string;
    onClick?: () => Promise<void> | void;
}

interface Payload {
    title: string;
    message: string;
    variant?: DialogVariant;
    icon?: FeatherIcon;
    actions: DialogAction[];
}

const variantConfig: Record<DialogVariant, { icon: FeatherIcon; color: string }> = {
    danger:  { icon: XCircle,       color: '#d9534f' },
    success: { icon: CheckCircle,   color: '#4BBF73' },
    warning: { icon: AlertTriangle, color: '#f0ad4e' },
    info:    { icon: Info,          color: '#3B82EC' },
};

const DialogContext = createContext<{ showDialog: (payload: Payload) => void }>({
    showDialog: () => {},
});

export const useDialog = () => useContext(DialogContext);

export const DialogProvider = (props: { children: ReactNode }) => {
    const [dialogContent, setDialogContent] = useState<Payload | undefined>();

    const variant = dialogContent?.variant ?? 'info';
    const config = variantConfig[variant];
    const Icon = dialogContent?.icon ?? config.icon;

    return (
        <DialogContext.Provider value={{ showDialog: (payload) => setDialogContent(payload) }}>
            <Modal
                show={!!dialogContent}
                onHide={() => setDialogContent(undefined)}
                centered
                size="sm"
            >
                <Modal.Body className="text-center px-4 py-4">
                    <div
                        style={{
                            width: 64,
                            height: 64,
                            borderRadius: '50%',
                            backgroundColor: `${config.color}14`,
                            border: `2px solid ${config.color}30`,
                            display: 'flex',
                            alignItems: 'center',
                            justifyContent: 'center',
                            margin: '0 auto 1rem',
                        }}
                    >
                        <Icon size={30} color={config.color} />
                    </div>

                    <h5 className="mb-2" style={{ fontWeight: 600 }}>
                        {dialogContent?.title}
                    </h5>

                    <p className="text-muted mb-0" style={{ fontSize: '0.9rem' }}>
                        {dialogContent?.message}
                    </p>
                </Modal.Body>

                {dialogContent?.actions && (
                    <Modal.Footer className="justify-content-center border-0 pt-0 pb-4" style={{ gap: '0.5rem' }}>
                        {dialogContent.actions.map((action, index) => (
                            <Button
                                key={index}
                                variant={action.variant ?? 'primary'}
                                size="sm"
                                style={{ minWidth: 100, fontWeight: 500 }}
                                onClick={async () => {
                                    try {
                                        setDialogContent(undefined);
                                        if (action.onClick) await action.onClick();
                                    } catch (err) {
                                        errorHandling(err);
                                    }
                                }}
                            >
                                {action.label}
                            </Button>
                        ))}
                    </Modal.Footer>
                )}
            </Modal>
            {props.children}
        </DialogContext.Provider>
    );
};