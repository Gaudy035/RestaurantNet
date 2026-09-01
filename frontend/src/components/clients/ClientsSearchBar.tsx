'use client';

import { useRouter } from 'next/navigation';
import { Input } from '../ui/input';
import React, { useState } from 'react';
import { Search } from 'lucide-react';
import { Button } from '../ui/button';

export default function AdminClientsSearchBar({ val }: { val?: string }) {
  const router = useRouter();
  const [clientName, setClientName] = useState<string>(val ?? '');

  const onSubmit = (e: React.SyntheticEvent<HTMLFormElement>) => {
    e.preventDefault();

    if (clientName) {
      setClientName(clientName.trim());
      router.push(`/admin/clients?param=${encodeURIComponent(clientName)}`);
    } else {
      router.push('/admin/clients');
    }
  };

  return (
    <form
      onSubmit={onSubmit}
      className='flex items-center justify-center gap-2'
    >
      <Input
        placeholder='Enter client data...'
        type='text'
        onChange={(e) => setClientName(e.target.value)}
        value={clientName}
      />
      <Button size='icon' type='submit'>
        <Search />
      </Button>
    </form>
  );
}
