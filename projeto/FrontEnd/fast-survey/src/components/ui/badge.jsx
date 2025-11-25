import React from 'react';
import styles from './shadcn.module.css';
import { cn } from '../../lib/cn';

export const Badge = ({ variant = 'default', className = '', children, ...props }) => (
  <span
    className={cn(
      styles.badge,
      variant === 'muted' && styles.badgeMuted,
      className
    )}
    {...props}
  >
    {children}
  </span>
);
