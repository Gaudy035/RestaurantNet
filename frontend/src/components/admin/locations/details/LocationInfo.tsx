'use client';

import { adminApiFetch } from '@/lib/api';
import { useState, useEffect } from 'react';
import LocationData from '@/interfaces/LocationData';
import {
  Card,
  CardDescription,
  CardHeader,
  CardTitle,
  CardAction,
} from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { useRouter } from 'next/navigation';

export default function LocationInfo({ locationId }: { locationId: string }) {
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const [locationInfo, setLocationInfo] = useState<LocationData | null>(null);
  const router = useRouter();

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

  const deleteLocation = async (locId: string) => {
    setError(null);
    try {
      await adminApiFetch(`/admin/locations/${locId}`, {
        method: 'DELETE',
      });
      router.push('/admin/locations');
    } catch (e: any) {
      setError(e?.message);
      console.log(e?.message);
    }
  };

  return (
    <div className='flex justify-center flex-col items-center'>
      <p className='text-destructive'>{error ? error : null}</p>
      {loading ? (
        <p>Loading location data...</p>
      ) : locationInfo ? (
        <Card className='flex w-full py-8 px-4'>
          <CardHeader className='flex flex-row justify-between items-center'>
            <div className='flex flex-col justify-center items-start gap-2'>
              <CardTitle className='flex justify-center items-center flex-row gap-4 text-2xl font-semibold'>
                {locationInfo.city}, {locationInfo.address}
              </CardTitle>
              <CardDescription className='flex flex-row justify-center items-center gap-4 text-lg'>
                Location ID: {locationInfo.locationId}
              </CardDescription>
            </div>

            <CardAction>
              <Button
                variant='destructive'
                className='text-lg p-4'
                size={'lg'}
                onClick={() => {
                  if (
                    window.confirm(
                      `Delete location: ${locationInfo.city}, ${locationInfo.address} (ID: ${locationInfo.locationId})?`,
                    )
                  ) {
                    deleteLocation(locationInfo.locationId);
                  }
                }}
              >
                Delete
              </Button>
            </CardAction>
          </CardHeader>
        </Card>
      ) : null}
    </div>
  );
}
