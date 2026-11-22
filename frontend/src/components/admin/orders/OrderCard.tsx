'use client';

import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { format } from 'date-fns';
import { cn } from '@/lib/utils';
import Order from '@/interfaces/Order';
import OrderType from '@/types/order-type';
import {
    Card,
    CardAction,
    CardDescription,
    CardHeader,
    CardTitle,
} from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { buttonVariants } from '@/components/ui/button';

const orderTypeLabels: Record<OrderType, string> = {
    DineIn: 'Dine in',
    Pickup: 'Pickup',
    Delivery: 'Delivery',
};

export default function OrderCard({
    orderData,
    locationLabel,
}: {
    orderData: Order;
    locationLabel: string;
}) {
    const router = useRouter();

    return (
        <Card
            className='group flex w-full px-2 py-5 cursor-pointer hover:bg-accent hover:text-accent-foreground transition'
            onClick={() => router.push(`/admin/orders/${orderData.orderId}`)}
        >
            <CardHeader className='flex flex-row justify-between items-center'>
                <div className='flex flex-col justify-center items-start gap-2'>
                    <CardTitle className='flex justify-center items-center flex-row gap-2'>
                        Order #{orderData.orderId}
                        <Badge>{orderData.orderStatus}</Badge>
                        <Badge variant='outline'>
                            {orderTypeLabels[orderData.orderType]}
                        </Badge>
                    </CardTitle>
                    <CardDescription className='flex flex-col justify-center items-start gap-1'>
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
                        <p>
                            Items:{' '}
                            {orderData.items
                                .map((i) => `${i.quantity}x ${i.name}`)
                                .join(', ')}
                        </p>
                    </CardDescription>
                </div>

                <CardAction>
                    <Link
                        href={`/admin/orders/${orderData.orderId}/update`}
                        onClick={(e) => e.stopPropagation()}
                        className={cn(
                            buttonVariants({
                                variant: 'default',
                                size: 'default',
                            }),
                        )}
                    >
                        Edit
                    </Link>
                </CardAction>
            </CardHeader>
        </Card>
    );
}
