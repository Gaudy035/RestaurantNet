'use client';

import React, { createContext, useContext } from 'react';
import EmployeeData from '@/interfaces/EmployeeData';

const AuthContext = createContext<EmployeeData | null>(null);

export function AuthProvider({
  employeeData,
  children,
}: {
  employeeData: EmployeeData;
  children: React.ReactNode;
}) {
  return (
    <AuthContext.Provider value={employeeData}>{children}</AuthContext.Provider>
  );
}

export function useAuth() {
  const user = useContext(AuthContext);
  return user;
}
