'use client';

import { adminApiFetch } from '@/lib/api';
import { useState, useEffect } from 'react';
import LocationData from '@/interfaces/LocationData';
import {
  Card,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';

export default function LocationInfo({ locationId }: { locationId: string }) {
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const [locationInfo, setLocationInfo] = useState<LocationData | null>(null);

  useEffect(() => {
    setLoading(true);

    adminApiFetch(`/admin/locations/${locationId}`, {
      method: 'GET',
      cache: 'no-store',
    })
      .then((res) => setLocationInfo(res))
      .catch((e: any) => setError(e?.message))
      .finally(() => setLoading(false));
  }, [locationId]);

  return (
    <div className='flex justify-center flex-col items-center'>
      <p className='text-destructive'>{error ? error : null}</p>
      {loading ? (
        <p>Loading location data...</p>
      ) : locationInfo ? (
        <Card className='w-full'>
          <CardHeader>
            <CardTitle className='text-2xl'>
              {locationInfo.city}, {locationInfo.address}
            </CardTitle>
            <CardDescription>
              Location ID: {locationInfo.locationId}
            </CardDescription>
          </CardHeader>
        </Card>
      ) : null}
    </div>
  );
}
