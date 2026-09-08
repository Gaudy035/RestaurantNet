'use client';

import { adminApiFetch } from '@/lib/api';
import ClientData from '@/interfaces/ClientData';
import {
  Card,
  CardAction,
  CardDescription,
  CardHeader,
  CardTitle,
} from '../../ui/card';
import { Button } from '../../ui/button';
import React from 'react';

export default function ClientCard({
  clientData,
  isAdmin,
  setError,
  handleDelete,
}: {
  clientData: ClientData;
  isAdmin: boolean;
  setError: React.Dispatch<React.SetStateAction<string | null>>;
  handleDelete: (userId: string) => void;
}) {
  const deleteUser = async (userId: string) => {
    setError(null);
    try {
      await adminApiFetch(`/admin/users/clients/${userId}`, {
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
          <CardTitle>
            {clientData.firstName} {clientData.lastName}
          </CardTitle>
          <CardDescription className='flex flex-row justify-center items-center gap-4'>
            <p>Email: {clientData.email}</p>
            <p>Phone number: {clientData.phoneNumber}</p>
          </CardDescription>
        </div>
        {isAdmin ? (
          <CardAction>
            <Button
              variant='destructive'
              className='font-semibold'
              size={'lg'}
              onClick={() => {
                if (
                  window.confirm(
                    `Delete user: ${clientData.firstName} ${clientData.lastName} (ID: ${clientData.userId})?`,
                  )
                ) {
                  deleteUser(clientData.userId);
                }
              }}
            >
              Delete
            </Button>
          </CardAction>
        ) : null}
      </CardHeader>
    </Card>
  );
}
