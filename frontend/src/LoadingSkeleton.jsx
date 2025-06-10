import React from "react";

export function LoadingSkeleton({ type = "card", count = 3 }) {
  if (type === "card") {
    return (
      <div className="challenges-grid">
        {Array.from({ length: count }).map((_, i) => (
          <div key={i} className="skeleton-card">
            <div className="skeleton-image"></div>
            <div className="skeleton-text skeleton-title"></div>
            <div className="skeleton-text skeleton-meta"></div>
          </div>
        ))}
      </div>
    );
  }

  if (type === "profile") {
    return (
      <div className="skeleton-profile">
        <div className="skeleton-avatar"></div>
        <div className="skeleton-text skeleton-name"></div>
        <div className="skeleton-text skeleton-email"></div>
      </div>
    );
  }

  return <div className="skeleton-text"></div>;
}