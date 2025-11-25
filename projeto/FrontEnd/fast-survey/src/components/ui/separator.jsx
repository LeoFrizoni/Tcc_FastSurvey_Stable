import React from 'react';
import styles from './shadcn.module.css';
import { cn } from '../../lib/cn';

export const Separator = ({ className = '', ...props }) => (
  <div className={cn(styles.separator, className)} {...props} />
);
