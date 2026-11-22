'use client';

import { useEffect, useState } from 'react';
import Order from '@/interfaces/Order';
import LocationData from '@/interfaces/LocationData';
import { adminApiFetch } from '@/lib/api';
import { getErrorMessage } from '@/lib/api-error';
import { Spinner } from '@/components/ui/spinner';
import OrderCard from '@/components/admin/orders/OrderCard';

export default function AdminOrdersMain({
    date,
    locationId,
}: {
    date?: string;
    locationId?: string;
}) {
    const [orders, setOrders] = useState<Order[]>([]);
    const [locations, setLocations] = useState<LocationData[]>([]);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        adminApiFetch<LocationData[]>('/admin/locations', {
            method: 'GET',
            cache: 'no-store',
        })
            .then((data) => setLocations(data))
            .catch((e) => setError(getErrorMessage(e)));
    }, []);

    useEffect(() => {
        const params = new URLSearchParams();

        if (date) {
            params.set('date', date);
        }

        if (locationId) {
            params.set('locationId', locationId);
        }

        const query = params.toString();

        adminApiFetch<Order[]>(
            `/admin/orders${query ? `?${query}` : ''}`,
            {
                method: 'GET',
                cache: 'no-store',
            },
        )
            .then((data) => {
                setError(null);
                setOrders(data);
            })
            .catch((e) => setError(getErrorMessage(e)))
            .finally(() => setLoading(false));
    }, [date, locationId]);

    const getLocationLabel = (id: string) => {
        const location = locations.find((l) => l.locationId === id);

        return location
            ? `${location.city}, ${location.address}`
            : `Location ID: ${id}`;
    };

    return (
        <div className='flex justify-center flex-col items-center'>
            <p className='text-destructive'>{error ? error : null}</p>
            {loading ? (
                <Spinner className='size-8' />
            ) : orders.length > 0 ? (
                <div className='flex flex-col m-8 w-full justify-center items-center gap-4'>
                    {orders.map((o) => (
                        <OrderCard
                            key={o.orderId}
                            orderData={o}
                            locationLabel={getLocationLabel(o.locationId)}
                        />
                    ))}
                </div>
            ) : (
                <p>No orders found</p>
            )}
        </div>
    );
}
