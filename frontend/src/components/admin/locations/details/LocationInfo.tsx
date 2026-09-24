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
import { getErrorMessage } from '@/lib/api-error';
import { toast } from 'sonner';
import ConfirmDialog from '../../AdminConfirmDialog';
import { Spinner } from '@/components/ui/spinner';

export default function LocationInfo({ locationId }: { locationId: string }) {
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [locationInfo, setLocationInfo] = useState<LocationData | null>(null);
  const router = useRouter();

  useEffect(() => {
    adminApiFetch<LocationData>(`/admin/locations/${locationId}`, {
      method: 'GET',
      cache: 'no-store',
    })
      .then((res) => setLocationInfo(res))
      .catch((e) => setError(getErrorMessage(e)))
      .finally(() => setLoading(false));
  }, [locationId]);

  const deleteLocation = async (locId: string) => {
    setError(null);
    try {
      await adminApiFetch(`/admin/locations/${locId}`, {
        method: 'DELETE',
      });
      toast.success('Location deleted successfully');
      router.push('/admin/locations');
    } catch (e) {
      setError(getErrorMessage(e));
    }
  };

  return (
    <div className='flex justify-center flex-col items-center'>
      <p className='text-destructive'>{error ? error : null}</p>
      {loading ? (
        <Spinner className='size-8' />
      ) : locationInfo ? (
        <Card className='flex w-full py-6 px-3'>
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
              <ConfirmDialog
                trigger={
                  <Button
                    variant='destructive'
                    className='text-lg p-4'
                    size={'lg'}
                  >
                    Delete
                  </Button>
                }
                variant='destructive'
                title='Delete location'
                description={`Delete location: ${locationInfo.city}, ${locationInfo.address} (ID: ${locationInfo.locationId})?`}
                onConfirm={() => deleteLocation(locationInfo.locationId)}
              />
            </CardAction>
          </CardHeader>
        </Card>
      ) : null}
    </div>
  );
}
