'use client';

import ClientData from '@/interfaces/ClientData';
import { adminApiFetch } from '@/lib/api';
import { useState, useEffect } from 'react';
import { useEmployee } from '@/lib/employee-context';
import ClientCard from './ClientCard';

export default function AdminClientsMain({
  clientData,
}: {
  clientData?: string;
}) {
  const [clients, setClients] = useState<ClientData[]>([]);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  const EmployeeContext = useEmployee();

  const handleClientDelete = (userId: string) => {
    setClients((prev) => prev.filter((c) => c.userId !== userId));
  };

  useEffect(() => {
    setLoading(true);

    const endpoint = clientData
      ? `/admin/user/clients?param=${clientData}`
      : '/admin/user/clients';

    adminApiFetch(endpoint, { method: 'GET', cache: 'no-store' })
      .then((data) => setClients(data))
      .catch((e: any) => setError(e?.message))
      .finally(() => setLoading(false));
  }, [clientData]);

  console.log(EmployeeContext);

  return (
    <div className='flex justify-center flex-col items-center'>
      <p className='text-destructive'>{error ? error : null}</p>
      {loading ? (
        <p>Loading clients data...</p>
      ) : clients.length > 0 ? (
        // Clients found
        <div className='flex flex-col m-8 w-full justify-center items-center gap-4'>
          {clients.map((c) => (
            <ClientCard
              key={c.userId}
              clientData={c}
              isAdmin={EmployeeContext.employeeData?.isAdmin ?? false}
              setError={setError}
              handleDelete={handleClientDelete}
            />
          ))}
        </div>
      ) : (
        // No clients found
        <p>No clients found</p>
      )}
    </div>
  );
}
