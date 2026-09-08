'use client';

import LocationData from '@/interfaces/LocationData';
import { adminApiFetch } from '@/lib/api';
import { useState, useEffect } from 'react';
import LocationCard from './LocationCard';

export default function AdminLocationsMain({
  locationData,
}: {
  locationData?: string;
}) {
  const [locations, setLocations] = useState<LocationData[]>([]);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    setLoading(true);

    const endpoint = locationData
      ? `/admin/locations?param=${locationData}`
      : '/admin/locations';

    adminApiFetch(endpoint, { method: 'GET', cache: 'no-store' })
      .then((data) => setLocations(data))
      .catch((e: any) => setError(e?.message))
      .finally(() => setLoading(false));
  }, [locationData]);

  return (
    <div className='flex justify-center flex-col items-center'>
      <p className='text-destructive'>{error ? error : null}</p>
      {loading ? (
        <p>Loading locations data...</p>
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
