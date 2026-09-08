'use client';

import EmployeeData from '@/interfaces/EmployeeData';
import { adminApiFetch } from '@/lib/api';
import { useState, useEffect } from 'react';
import EmployeeCard from './EmployeeCard';
import { useEmployee } from '@/lib/employee-context';

export default function AdminEmployeesMain({
  employeeData,
}: {
  employeeData?: string;
}) {
  const [employees, setEmployees] = useState<EmployeeData[]>([]);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  const employeeContext = useEmployee();

  const handleEmployeeDelete = (userId: string) => {
    setEmployees((prev) => prev.filter((e) => e.userId !== userId));
  };

  useEffect(() => {
    setLoading(true);

    const endpoint = employeeData
      ? `/admin/users/employees?param=${employeeData}`
      : '/admin/users/employees';

    adminApiFetch(endpoint, { method: 'GET', cache: 'no-store' })
      .then((data) => setEmployees(data))
      .catch((e: any) => setError(e?.message))
      .finally(() => setLoading(false));
  }, [employeeData]);

  return (
    <div className='flex justify-center flex-col items-center'>
      <p className='text-destructive'>{error ? error : null}</p>
      {loading ? (
        <p>Loading employee data...</p>
      ) : employees.length > 0 ? (
        // Employees found
        <div className='flex flex-col m-8 w-full justify-center items-center gap-4'>
          {employees.map((e) => (
            <EmployeeCard
              key={e.userId}
              employeeData={e}
              currentId={employeeContext.employeeData!.userId}
              setError={setError}
              handleDelete={handleEmployeeDelete}
            />
          ))}
        </div>
      ) : (
        // No employees found
        <p>No employees found</p>
      )}
    </div>
  );
}
