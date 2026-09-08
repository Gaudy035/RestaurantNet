'use client';

import { useRouter } from 'next/navigation';
import { Input } from '../ui/input';
import React, { useState } from 'react';
import { Search } from 'lucide-react';
import { Button } from '../ui/button';

export default function AdminLocationsSearchBar({ val }: { val?: string }) {
  const router = useRouter();
  const [locationData, setLocationData] = useState<string>(val ?? '');

  const onSubmit = (e: React.SyntheticEvent<HTMLFormElement>) => {
    e.preventDefault();

    if (locationData) {
      setLocationData(locationData.trim());
      router.push(`/admin/locations?param=${encodeURIComponent(locationData)}`);
    } else {
      router.push('/admin/locations');
    }
  };

  return (
    <form
      onSubmit={onSubmit}
      className='flex items-center justify-center gap-2'
    >
      <Input
        placeholder='Enter location data...'
        type='text'
        onChange={(e) => setLocationData(e.target.value)}
        value={locationData}
      />
      <Button size='icon' type='submit'>
        <Search />
      </Button>
    </form>
  );
}
