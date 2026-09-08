'use client';

import LocationData from '@/interfaces/LocationData';
import {
  Card,
  CardAction,
  CardDescription,
  CardHeader,
  CardTitle,
} from '../../ui/card';
import { Button } from '../../ui/button';
import { useRouter } from 'next/navigation';

export default function LocationCard({
  locationData,
}: {
  locationData: LocationData;
}) {
  const router = useRouter();

  return (
    <Card className='flex w-full py-4'>
      <CardHeader className='flex flex-row justify-between items-center'>
        <div className='flex flex-col justify-center items-start gap-2'>
          <CardTitle className='flex justify-center items-center flex-row gap-2'>
            <p>{locationData.city}</p>
          </CardTitle>
          <CardDescription className='flex flex-row justify-center items-center gap-4'>
            <p>{locationData.address}</p>
          </CardDescription>
        </div>
        <CardAction>
          <Button
            variant='default'
            size={'lg'}
            onClick={() => {
              router.push(`/admin/locations/${locationData.locationId}`);
            }}
          >
            Details
          </Button>
        </CardAction>
      </CardHeader>
    </Card>
  );
}
