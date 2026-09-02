'use client';

import React, { createContext, useContext } from 'react';
import EmployeeData from '@/interfaces/EmployeeData';

const EmployeeContext = createContext<EmployeeData | null>(null);

export function EmployeeProvider({
  employeeData,
  children,
}: {
  employeeData: EmployeeData;
  children: React.ReactNode;
}) {
  return (
    <EmployeeContext.Provider value={employeeData}>
      {children}
    </EmployeeContext.Provider>
  );
}

export function useEmployee() {
  const employee = useContext(EmployeeContext);
  return employee;
}
