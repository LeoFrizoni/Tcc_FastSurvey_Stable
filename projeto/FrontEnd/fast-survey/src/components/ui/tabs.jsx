import React, { createContext, useContext, useMemo, useState } from 'react';
import styles from './shadcn.module.css';
import { cn } from '../../lib/cn';

const TabsContext = createContext(null);

export const Tabs = ({ defaultValue, value, onValueChange, children, className = '' }) => {
  const [internalValue, setInternalValue] = useState(defaultValue);
  const currentValue = value ?? internalValue ?? defaultValue;

  const contextValue = useMemo(
    () => ({
      value: currentValue,
      setValue: (next) => {
        setInternalValue(next);
        onValueChange?.(next);
      },
    }),
    [currentValue, onValueChange]
  );

  return (
    <TabsContext.Provider value={contextValue}>
      <div className={cn(styles.tabsRoot, className)}>{children}</div>
    </TabsContext.Provider>
  );
};

const useTabsContext = () => {
  const ctx = useContext(TabsContext);
  if (!ctx) throw new Error('Tabs components must be used within <Tabs />');
  return ctx;
};

export const TabsList = ({ className = '', children }) => (
  <div className={cn(styles.tabsList, className)}>{children}</div>
);

export const TabsTrigger = ({ value, className = '', children }) => {
  const { value: activeValue, setValue } = useTabsContext();
  const isActive = activeValue === value;

  return (
    <button
      type="button"
      className={cn(styles.tabsTrigger, isActive && styles.tabsTriggerActive, className)}
      onClick={() => setValue(value)}
    >
      {children}
    </button>
  );
};

export const TabsContent = ({ value, className = '', children }) => {
  const { value: activeValue } = useTabsContext();
  if (activeValue !== value) return null;

  return <div className={cn(styles.tabsContent, className)}>{children}</div>;
};
