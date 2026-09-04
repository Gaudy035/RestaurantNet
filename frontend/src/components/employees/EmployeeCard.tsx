'use client';

import { adminApiFetch } from '@/lib/api';
import EmployeeData from '@/interfaces/EmployeeData';
import {
  Card,
  CardAction,
  CardDescription,
  CardHeader,
  CardTitle,
} from '../ui/card';
import { Button } from '../ui/button';
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
  const deleteUser = async (userId: string) => {
    setError(null);
    try {
      await adminApiFetch(`/admin/user/employees/${userId}`, {
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
          <CardTitle className='flex flex-row gap-2'>
            {employeeData.firstName} {employeeData.lastName}
            {employeeData.isAdmin ? ' [Administrator]' : null}
          </CardTitle>
          <CardDescription className='flex flex-row justify-center items-center gap-4'>
            <p>Email: {employeeData.email}</p>
          </CardDescription>
        </div>

        <CardAction>
          {currentId !== employeeData.userId ? (
            <Button
              className='font-semibold bg-destructive hover:text-destructive hover:border-destructive hover:bg-primary'
              size={'lg'}
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
              Delete
            </Button>
          ) : null}
        </CardAction>
      </CardHeader>
    </Card>
  );
}
