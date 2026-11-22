'use client';

import { useEffect, useState } from 'react';
import Link from 'next/link';
import { format } from 'date-fns';
import { adminApiFetch } from '@/lib/api';
import { getErrorMessage } from '@/lib/api-error';
import { cn } from '@/lib/utils';
import Order from '@/interfaces/Order';
import LocationData from '@/interfaces/LocationData';
import OrderType from '@/types/order-type';
import {
    Card,
    CardAction,
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle,
} from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { buttonVariants } from '@/components/ui/button';
import { Spinner } from '@/components/ui/spinner';

const orderTypeLabels: Record<OrderType, string> = {
    DineIn: 'Dine in',
    Pickup: 'Pickup',
    Delivery: 'Delivery',
};

export default function OrderInfo({ orderId }: { orderId: string }) {
    const [orderData, setOrderData] = useState<Order | null>(null);
    const [locations, setLocations] = useState<LocationData[]>([]);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        adminApiFetch<Order>(`/admin/orders/${orderId}`, {
            method: 'GET',
            cache: 'no-store',
        })
            .then((data) => setOrderData(data))
            .catch((e) => setError(getErrorMessage(e)))
            .finally(() => setLoading(false));
    }, [orderId]);

    useEffect(() => {
        adminApiFetch<LocationData[]>('/admin/locations', {
            method: 'GET',
            cache: 'no-store',
        })
            .then((data) => setLocations(data))
            .catch(() => setLocations([]));
    }, []);

    const location = orderData
        ? locations.find((l) => l.locationId === orderData.locationId)
        : undefined;
    const locationLabel = orderData
        ? location
            ? `${location.city}, ${location.address}`
            : `Location ID: ${orderData.locationId}`
        : '';

    return (
        <div className='flex justify-center flex-col items-center'>
            <p className='text-destructive'>{error ? error : null}</p>
            {loading ? (
                <Spinner className='size-8' />
            ) : orderData ? (
                <Card className='flex w-full py-6 px-3'>
                    <CardHeader className='flex flex-row justify-between items-center'>
                        <div className='flex flex-col justify-center items-start gap-2'>
                            <CardTitle className='flex justify-center items-center flex-row gap-4 text-2xl font-semibold'>
                                Order #{orderData.orderId}
                                <Badge>{orderData.orderStatus}</Badge>
                                <Badge variant='outline'>
                                    {orderTypeLabels[orderData.orderType]}
                                </Badge>
                            </CardTitle>
                            <CardDescription className='flex flex-col justify-center items-start gap-2 text-lg'>
                                <p>
                                    Time:{' '}
                                    {format(
                                        new Date(orderData.orderTime),
                                        'dd.MM.yyyy HH:mm',
                                    )}
                                </p>
                                <p>Location: {locationLabel}</p>
                                <p>Total: {orderData.price.toFixed(2)}</p>
                                <p>
                                    Payment: {orderData.paymentMethod} (
                                    {orderData.isPaid ? 'Paid' : 'Unpaid'})
                                </p>
                                {orderData.phoneNumber ? (
                                    <p>Phone: {orderData.phoneNumber}</p>
                                ) : null}
                                {orderData.clientId ? (
                                    <p>Client ID: {orderData.clientId}</p>
                                ) : null}
                                {orderData.delivery ? (
                                    <p>
                                        Delivery: {orderData.delivery.city},{' '}
                                        {orderData.delivery.address}
                                    </p>
                                ) : null}
                            </CardDescription>
                        </div>

                        <CardAction>
                            <Link
                                href={`/admin/orders/${orderId}/update`}
                                className={cn(
                                    buttonVariants({
                                        variant: 'default',
                                        size: 'default',
                                    }),
                                )}
                            >
                                Modify order
                            </Link>
                        </CardAction>
                    </CardHeader>
                    <CardContent>
                        <p className='text-lg mb-2'>Items</p>
                        <div className='divide-y'>
                            {orderData.items.map((item) => (
                                <div
                                    key={item.itemId}
                                    className='flex flex-row justify-between items-center w-full py-1'
                                >
                                    <p>
                                        {item.quantity}x {item.name}
                                    </p>
                                    <p>
                                        {(
                                            item.price * item.quantity
                                        ).toFixed(2)}
                                    </p>
                                </div>
                            ))}
                        </div>
                    </CardContent>
                </Card>
            ) : null}
        </div>
    );
}
