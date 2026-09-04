'use client';

import { useRouter } from 'next/navigation';
import { Input } from '../ui/input';
import React, { useState } from 'react';
import { Search } from 'lucide-react';
import { Button } from '../ui/button';

export default function AdminEmployeesSearchBar({ val }: { val?: string }) {
  const router = useRouter();
  const [employeeName, setEmployeeName] = useState<string>(val ?? '');

  const onSubmit = (e: React.SyntheticEvent<HTMLFormElement>) => {
    e.preventDefault();

    if (employeeName) {
      setEmployeeName(employeeName.trim());
      router.push(`/admin/employees?param=${encodeURIComponent(employeeName)}`);
    } else {
      router.push('/admin/employees');
    }
  };

  return (
    <form
      onSubmit={onSubmit}
      className='flex items-center justify-center gap-2'
    >
      <Input
        placeholder='Enter employee data...'
        type='text'
        onChange={(e) => setEmployeeName(e.target.value)}
        value={employeeName}
      />
      <Button size='icon' type='submit'>
        <Search />
      </Button>
    </form>
  );
}
