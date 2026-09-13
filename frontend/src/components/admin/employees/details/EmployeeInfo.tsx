'use client';

import EmployeeData from '@/interfaces/EmployeeData';
import { adminApiFetch } from '@/lib/api';
import { useState, useEffect } from 'react';
import { useEmployee } from '@/lib/employee-context';
import { Card } from '@/components/ui/card';

export default function EmployeeInfo({ employeeId }: { employeeId: string }) {
  const loggedInEmployee = useEmployee();

  const [employeeData, setEmployeeData] = useState<null | EmployeeData>(null);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<null | string>(null);

  useEffect(() => {
    setLoading(true);

    // adminApiFetch();
  }, [employeeId]);

  return <Card></Card>;
}
