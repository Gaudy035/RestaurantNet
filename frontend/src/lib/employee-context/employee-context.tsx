'use client';

import React, { createContext, useContext } from 'react';
import EmployeeData from '@/interfaces/EmployeeData';
import { adminApiFetch } from '../api';
import { useState, useEffect } from 'react';

const EmployeeContext = createContext<{
  employeeData: EmployeeData | null;
  loading: boolean;
}>({ employeeData: null, loading: true });

export function EmployeeProvider({ children }: { children: React.ReactNode }) {
  const [employeeData, setEmployeeData] = useState<EmployeeData | null>(null);
  const [loading, setLoading] = useState<boolean>(true);

  useEffect(() => {
    adminApiFetch('/admin/auth/me', {
      method: 'GET',
      cache: 'no-store',
    })
      .then(setEmployeeData)
      .catch(() => setEmployeeData(null))
      .finally(() => setLoading(false));
  }, []);

  return (
    <EmployeeContext.Provider value={{ employeeData, loading }}>
      {children}
    </EmployeeContext.Provider>
  );
}

export function useEmployee() {
  const employee = useContext(EmployeeContext);
  return employee;
}
