import React, { createContext, useContext, useState } from "react";

const ToastContext = createContext();

export function ToastProvider({ children }) {
  const [toasts, setToasts] = useState([]);
  const MAX_TOASTS = 3;

  const addToast = (message, type = "info", duration = 3000) => {
    const id = Date.now() + Math.random();
    const toast = { id, message, type, duration };
    
    setToasts(prev => {
      const newToasts = prev.length >= MAX_TOASTS ? prev.slice(1) : prev;
      return [...newToasts, toast];
    });
    
    setTimeout(() => {
      removeToast(id);
    }, duration);
  };

  const removeToast = (id) => {
    setToasts(prev => prev.filter(toast => toast.id !== id));
  };

  const showSuccess = (message) => {
    const exists = toasts.some(toast => toast.message === message && toast.type === "success");
    if (!exists) {
      addToast(message, "success");
    }
  };

  const showError = (message) => {
    const exists = toasts.some(toast => toast.message === message && toast.type === "error");
    if (!exists) {
      addToast(message, "error");
    }
  };

  const showInfo = (message) => {
    const exists = toasts.some(toast => toast.message === message && toast.type === "info");
    if (!exists) {
      addToast(message, "info");
    }
  };

  const showWarning = (message) => {
    const exists = toasts.some(toast => toast.message === message && toast.type === "warning");
    if (!exists) {
      addToast(message, "warning");
    }
  };

  const clearAllToasts = () => {
    setToasts([]);
  };

  return (
    <ToastContext.Provider value={{ 
      toasts, 
      addToast, 
      removeToast, 
      showSuccess, 
      showError, 
      showInfo, 
      showWarning,
      clearAllToasts
    }}>
      {children}
      <ToastContainer />
    </ToastContext.Provider>
  );
}

function ToastContainer() {
  const { toasts, removeToast, clearAllToasts } = useContext(ToastContext);

  if (toasts.length === 0) return null;

  return (
    <div className="toast-container">
      {toasts.length > 1 && (
        <button 
          className="clear-all-toasts"
          onClick={clearAllToasts}
          title="Clear all notifications"
        >
          Clear All ({toasts.length})
        </button>
      )}
      {toasts.map(toast => (
        <div 
          key={toast.id} 
          className={`toast toast-${toast.type}`}
          onClick={() => removeToast(toast.id)}
        >
          <span className="toast-icon">
            {toast.type === "success" && "✅"}
            {toast.type === "error" && "❌"}
            {toast.type === "warning" && "⚠️"}
            {toast.type === "info" && "ℹ️"}
          </span>
          <span className="toast-message">{toast.message}</span>
          <button 
            className="toast-close"
            onClick={(e) => {
              e.stopPropagation();
              removeToast(toast.id);
            }}
          >
            ×
          </button>
        </div>
      ))}
    </div>
  );
}

export function useToast() {
  const context = useContext(ToastContext);
  if (!context) {
    throw new Error("useToast must be used within ToastProvider");
  }
  return context;
}