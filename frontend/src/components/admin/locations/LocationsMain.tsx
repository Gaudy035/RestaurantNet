'use client';

import LocationData from '@/interfaces/LocationData';
import { adminApiFetch } from '@/lib/api';
import { useState, useEffect } from 'react';
import LocationCard from './LocationCard';
import { getErrorMessage } from '@/lib/api-error';
import { Spinner } from '@/components/ui/spinner';

export default function AdminLocationsMain({
  locationData,
}: {
  locationData?: string;
}) {
  const [locations, setLocations] = useState<LocationData[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const endpoint = locationData
      ? `/admin/locations?param=${encodeURIComponent(locationData)}`
      : '/admin/locations';

    adminApiFetch<LocationData[]>(endpoint, {
      method: 'GET',
      cache: 'no-store',
    })
      .then((data) => setLocations(data))
      .catch((e) => setError(getErrorMessage(e)))
      .finally(() => setLoading(false));
  }, [locationData]);

  return (
    <div className='flex justify-center flex-col items-center'>
      <p className='text-destructive'>{error ? error : null}</p>
      {loading ? (
        <Spinner className='size-8' />
      ) : locations.length > 0 ? (
        // Locations found
        <div className='flex flex-col m-8 w-full justify-center items-center gap-4'>
          {locations.map((l) => (
            <LocationCard key={l.locationId} locationData={l} />
          ))}
        </div>
      ) : (
        // No locations found
        <p>No locations found</p>
      )}
    </div>
  );
}
