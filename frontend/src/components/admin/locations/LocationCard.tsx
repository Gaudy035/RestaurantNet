'use client';

import { adminApiFetch } from '@/lib/api';
import LocationData from '@/interfaces/LocationData';
import {
  Card,
  CardAction,
  CardDescription,
  CardHeader,
  CardTitle,
} from '../../ui/card';
import { Badge } from '../../ui/badge';
import { Button } from '../../ui/button';
import React from 'react';

export default function LocationCard({
  locationData,
}: {
  locationData: LocationData;
}) {
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
      </CardHeader>
    </Card>
  );
}
