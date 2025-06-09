import React from "react";
import { useTheme } from "./ThemeContext";

export function ThemeToggle() {
  const { isDarkMode, toggleDarkMode } = useTheme();

  return (
    <button
      onClick={toggleDarkMode}
      className="theme-toggle"
      title={`Switch to ${isDarkMode ? 'light' : 'dark'} mode`}
      aria-label={`Switch to ${isDarkMode ? 'light' : 'dark'} mode`}
    >
      <span className="theme-icon">
        {isDarkMode ? "☀️" : "🌙"}
      </span>
    </button>
  );
}