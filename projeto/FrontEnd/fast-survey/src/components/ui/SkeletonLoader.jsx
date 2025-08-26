import React from 'react';
import './SkeletonLoader.module.css';

const SkeletonLoader = ({ type = 'card', lines = 3, height = 'auto' }) => {
  const renderSkeleton = () => {
    switch (type) {
      case 'card':
        return (
          <div className="skeleton-card">
            <div className="skeleton-header">
              <div className="skeleton-badge shimmer"></div>
              <div className="skeleton-tag shimmer"></div>
            </div>
            <div className="skeleton-title shimmer"></div>
            <div className="skeleton-description shimmer"></div>
            <div className="skeleton-footer">
              <div className="skeleton-meta shimmer"></div>
              <div className="skeleton-actions">
                <div className="skeleton-button shimmer"></div>
                <div className="skeleton-button shimmer"></div>
              </div>
            </div>
          </div>
        );
      
      case 'text':
        return (
          <div className="skeleton-text">
            {Array.from({ length: lines }).map((_, index) => (
              <div 
                key={index} 
                className="skeleton-line shimmer"
                style={{ 
                  width: `${Math.random() * 40 + 60}%`,
                  height: height === 'auto' ? '16px' : height
                }}
              ></div>
            ))}
          </div>
        );
      
      case 'button':
        return (
          <div className="skeleton-button shimmer" style={{ height }}></div>
        );
      
      case 'input':
        return (
          <div className="skeleton-input shimmer" style={{ height }}></div>
        );
      
      default:
        return (
          <div className="skeleton-default shimmer" style={{ height }}></div>
        );
    }
  };

  return (
    <div className="skeleton-container">
      {renderSkeleton()}
    </div>
  );
};

export default SkeletonLoader;
