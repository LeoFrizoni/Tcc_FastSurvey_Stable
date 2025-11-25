import React from 'react';
import styles from './shadcn.module.css';
import { cn } from '../../lib/cn';

export const Card = ({ className = '', children, ...props }) => (
  <div className={cn(styles.card, className)} {...props}>
    {children}
  </div>
);

export const CardHeader = ({ className = '', children, ...props }) => (
  <div className={cn(styles.cardHeader, className)} {...props}>
    {children}
  </div>
);

export const CardContent = ({ className = '', children, ...props }) => (
  <div className={cn(styles.cardContent, className)} {...props}>
    {children}
  </div>
);

export const CardFooter = ({ className = '', children, ...props }) => (
  <div className={cn(styles.cardFooter, className)} {...props}>
    {children}
  </div>
);

export const CardTitle = ({ className = '', children, ...props }) => (
  <h3 className={cn(styles.cardTitle, className)} {...props}>
    {children}
  </h3>
);

export const CardDescription = ({ className = '', children, ...props }) => (
  <p className={cn(styles.cardDescription, className)} {...props}>
    {children}
  </p>
);
