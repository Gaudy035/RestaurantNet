'use client';

import ClientData from '@/interfaces/ClientData';
import { adminApiFetch } from '@/lib/api';
import { useState, useEffect } from 'react';
import { useEmployee } from '@/lib/employee-context/employee-context';

export default function AdminClientsMain({
  clientData,
}: {
  clientData?: string;
}) {
  const [clients, setClients] = useState<ClientData[]>([]);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  const EmployeeContext = useEmployee();

  useEffect(() => {
    setLoading(true);

    const endpoint = clientData
      ? `/admin/user/clients?param=${clientData}`
      : '/admin/user/clients';

    adminApiFetch(endpoint, { method: 'GET', cache: 'no-store' })
      .then((data) => setClients(data))
      .catch((e) => setError(e))
      .finally(() => setLoading(false));
  }, [clientData]);

  return (
    <div>
      <h1>{error ? error : null}</h1>
      {loading ? 'loading' : clientData}
    </div>
  );
}
