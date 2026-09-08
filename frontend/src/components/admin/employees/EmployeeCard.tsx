'use client';

import { adminApiFetch } from '@/lib/api';
import EmployeeData from '@/interfaces/EmployeeData';
import {
  Card,
  CardAction,
  CardDescription,
  CardHeader,
  CardTitle,
} from '../../ui/card';
import { Badge } from '../../ui/badge';
import { Button } from '../../ui/button';
import React from 'react';

export default function EmployeeCard({
  employeeData,
  currentId,
  setError,
  handleDelete,
}: {
  employeeData: EmployeeData;
  currentId: string;
  setError: React.Dispatch<React.SetStateAction<string | null>>;
  handleDelete: (userId: string) => void;
}) {
  const isSelf = currentId === employeeData.userId;

  const deleteUser = async (userId: string) => {
    setError(null);
    try {
      await adminApiFetch(`/admin/users/employees/${userId}`, {
        method: 'DELETE',
      });
      handleDelete(userId);
    } catch (e: any) {
      setError(e?.message);
      console.log(e?.message);
    }
  };

  return (
    <Card className='flex w-full py-4'>
      <CardHeader className='flex flex-row justify-between items-center'>
        <div className='flex flex-col justify-center items-start gap-2'>
          <CardTitle className='flex justify-center items-center flex-row gap-2'>
            {employeeData.firstName} {employeeData.lastName}
            {employeeData.isAdmin ? <Badge>Administrator</Badge> : null}
          </CardTitle>
          <CardDescription className='flex flex-row justify-center items-center gap-4'>
            <p>Email: {employeeData.email}</p>
          </CardDescription>
        </div>

        <CardAction>
          <Button
            variant={isSelf ? 'outline' : 'destructive'}
            className='font-semibold'
            size={'lg'}
            disabled={isSelf}
            onClick={() => {
              if (
                window.confirm(
                  `Delete user: ${employeeData.firstName} ${employeeData.lastName} (ID: ${employeeData.userId})?`,
                )
              ) {
                deleteUser(employeeData.userId);
              }
            }}
          >
            {isSelf ? 'You' : 'Delete'}
          </Button>
        </CardAction>
      </CardHeader>
    </Card>
  );
}
