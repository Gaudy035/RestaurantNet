'use client';

import EmployeeData from '@/interfaces/EmployeeData';
import { adminApiFetch } from '@/lib/api';
import { useState, useEffect } from 'react';
import {
  Card,
  CardAction,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { useEmployee } from '@/lib/employee-context';
import { useRouter } from 'next/navigation';
import { getErrorMessage } from '@/lib/api-error';
import { toast } from 'sonner';
import ConfirmDialog from '../../AdminConfirmDialog';

export default function EmployeeInfo({ employeeId }: { employeeId: string }) {
  const [employeeInfo, setEmployeeInfo] = useState<null | EmployeeData>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<null | string>(null);

  const router = useRouter();
  const currentEmployee = useEmployee();

  useEffect(() => {
    adminApiFetch<EmployeeData>(`/admin/users/employees/${employeeId}`, {
      method: 'GET',
      cache: 'no-store',
    })
      .then((res) => {
        setError(null);
        setEmployeeInfo(res);
      })
      .catch((e) => setError(getErrorMessage(e)))
      .finally(() => setLoading(false));
  }, [employeeId]);

  const isSelf: boolean =
    currentEmployee.employeeData?.userId === employeeInfo?.userId &&
    !!employeeInfo?.userId &&
    !!currentEmployee.employeeData?.userId;

  const deleteUser = async (userId: string) => {
    setError(null);
    try {
      await adminApiFetch(`/admin/users/employees/${userId}`, {
        method: 'DELETE',
      });
      toast.success('Employee deleted successfully');
      router.push('/admin/employees');
    } catch (e) {
      setError(getErrorMessage(e));
    }
  };

  return (
    <div className='flex justify-center flex-col items-center'>
      <p className='text-destructive'>{error ? error : null}</p>
      {loading ? (
        <p>Loading employee data...</p>
      ) : employeeInfo ? (
        <Card className='flex w-full py-6 px-3'>
          <CardHeader className='flex flex-row justify-between items-center'>
            <div className='flex flex-col justify-center items-start gap-2'>
              <CardTitle className='flex justify-center items-center flex-row gap-4 text-2xl font-semibold'>
                {employeeInfo.firstName} {employeeInfo.lastName}
                {employeeInfo.isAdmin ? <Badge>Administrator</Badge> : null}
              </CardTitle>
              <CardDescription className='flex flex-col justify-center items-start text-lg'>
                <p>Email: {employeeInfo.email}</p>
                <p>User ID: {employeeInfo.userId}</p>
              </CardDescription>
            </div>

            <CardAction>
              <ConfirmDialog
                trigger={
                  <Button
                    variant={isSelf ? 'outline' : 'destructive'}
                    className='text-lg p-4'
                    size={'lg'}
                    disabled={isSelf}
                  >
                    {isSelf ? 'You' : 'Delete'}
                  </Button>
                }
                variant='destructive'
                title='Delete employee'
                description={`Delete user: ${employeeInfo.firstName} ${employeeInfo.lastName} (ID: ${employeeInfo.userId})?`}
                onConfirm={() => deleteUser(employeeInfo.userId)}
              />
            </CardAction>
          </CardHeader>
        </Card>
      ) : null}
    </div>
  );
}
