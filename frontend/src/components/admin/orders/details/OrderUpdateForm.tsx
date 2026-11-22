'use client';

import React, { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { toast } from 'sonner';
import { adminApiFetch } from '@/lib/api';
import { getErrorMessage } from '@/lib/api-error';
import Order from '@/interfaces/Order';
import OrderStatus from '@/types/order-status';
import PaymentMethod from '@/types/payment-method';
import {
    Card,
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle,
} from '@/components/ui/card';
import { Field, FieldGroup, FieldLabel } from '@/components/ui/field';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import {
    Select,
    SelectContent,
    SelectGroup,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from '@/components/ui/select';
import { Spinner } from '@/components/ui/spinner';

const orderStatuses: { value: OrderStatus; label: string }[] = [
    { value: 'Pending', label: 'Pending' },
    { value: 'Preparing', label: 'Preparing' },
    { value: 'Ready', label: 'Ready' },
    { value: 'Shipped', label: 'Shipped' },
    { value: 'Completed', label: 'Completed' },
    { value: 'Cancelled', label: 'Cancelled' },
];

const paymentMethods: { value: PaymentMethod; label: string }[] = [
    { value: 'Card', label: 'Card' },
    { value: 'Cash', label: 'Cash' },
];

export default function AdminOrderUpdateForm({ orderId }: { orderId: string }) {
    const router = useRouter();
    const [orderData, setOrderData] = useState<Order | null>(null);
    const [loading, setLoading] = useState<boolean>(true);
    const [submitting, setSubmitting] = useState<boolean>(false);
    const [error, setError] = useState<string | null>(null);
    const [orderStatus, setOrderStatus] = useState<OrderStatus | null>(null);
    const [paymentMethod, setPaymentMethod] = useState<PaymentMethod | null>(
        null,
    );

    useEffect(() => {
        adminApiFetch<Order>(`/admin/orders/${orderId}`, {
            method: 'GET',
            cache: 'no-store',
        })
            .then((data) => {
                setOrderData(data);
                setOrderStatus(data.orderStatus);
                setPaymentMethod(data.paymentMethod);
            })
            .catch((e) => setError(getErrorMessage(e)))
            .finally(() => setLoading(false));
    }, [orderId]);

    const onSubmit = async (e: React.SyntheticEvent<HTMLFormElement>) => {
        e.preventDefault();
        setError(null);

        if (!orderData) {
            return;
        }

        const formData = new FormData(e.currentTarget);

        const payload: {
            paymentMethod?: PaymentMethod;
            isPaid?: boolean;
            status?: OrderStatus;
            phoneNumber?: string;
            city?: string;
            address?: string;
        } = {};

        if (
            paymentMethod &&
            paymentMethod !== orderData.paymentMethod
        ) {
            payload.paymentMethod = paymentMethod;
        }

        if (orderStatus && orderStatus !== orderData.orderStatus) {
            payload.status = orderStatus;
        }

        const phoneNumber =
            formData.get('phoneNumber')?.toString().trim() ?? '';
        if (phoneNumber && phoneNumber !== orderData.phoneNumber) {
            payload.phoneNumber = phoneNumber;
        }

        const isPaid = formData.get('isPaid') === 'on';
        if (isPaid !== orderData.isPaid) {
            payload.isPaid = isPaid;
        }

        if (orderData.orderType === 'Delivery') {
            const city = formData.get('city')?.toString().trim() ?? '';
            const address = formData.get('address')?.toString().trim() ?? '';

            if (city && city !== orderData.delivery?.city) {
                payload.city = city;
            }

            if (address && address !== orderData.delivery?.address) {
                payload.address = address;
            }
        }

        if (Object.keys(payload).length === 0) {
            setError('No changes to save');
            return;
        }

        setSubmitting(true);

        try {
            await adminApiFetch(`/admin/orders/${orderId}`, {
                method: 'PATCH',
                body: JSON.stringify(payload),
            });

            toast.success('Order updated successfully');
            router.push(`/admin/orders/${orderId}`);
        } catch (err) {
            setError(getErrorMessage(err));
        } finally {
            setSubmitting(false);
        }
    };

    if (loading) {
        return <Spinner className='size-8' />;
    }

    if (!orderData) {
        return <p className='text-destructive'>{error ?? 'Order not found'}</p>;
    }

    return (
        <Card>
            <CardHeader>
                <CardTitle>Update an order</CardTitle>
                <CardDescription>
                    Order #{orderData.orderId} ({orderData.orderType})
                </CardDescription>
            </CardHeader>
            <CardContent>
                <form onSubmit={onSubmit}>
                    <FieldGroup>
                        <Field>
                            <FieldLabel htmlFor='status'>Status</FieldLabel>
                            <Select
                                items={orderStatuses}
                                value={orderStatus}
                                onValueChange={(value) =>
                                    value && setOrderStatus(value as OrderStatus)
                                }
                            >
                                <SelectTrigger id='status' className='w-full'>
                                    <SelectValue placeholder='Select status' />
                                </SelectTrigger>
                                <SelectContent>
                                    <SelectGroup>
                                        {orderStatuses.map((os) => (
                                            <SelectItem
                                                key={os.value}
                                                value={os.value}
                                            >
                                                {os.label}
                                            </SelectItem>
                                        ))}
                                    </SelectGroup>
                                </SelectContent>
                            </Select>
                        </Field>
                        <Field>
                            <FieldLabel htmlFor='paymentMethod'>
                                Payment method
                            </FieldLabel>
                            <Select
                                items={paymentMethods}
                                value={paymentMethod}
                                onValueChange={(value) =>
                                    value &&
                                    setPaymentMethod(value as PaymentMethod)
                                }
                            >
                                <SelectTrigger
                                    id='paymentMethod'
                                    className='w-full'
                                >
                                    <SelectValue placeholder='Select payment method' />
                                </SelectTrigger>
                                <SelectContent>
                                    <SelectGroup>
                                        {paymentMethods.map((pm) => (
                                            <SelectItem
                                                key={pm.value}
                                                value={pm.value}
                                            >
                                                {pm.label}
                                            </SelectItem>
                                        ))}
                                    </SelectGroup>
                                </SelectContent>
                            </Select>
                        </Field>
                        <Field>
                            <FieldLabel htmlFor='phoneNumber'>
                                Phone number
                            </FieldLabel>
                            <Input
                                id='phoneNumber'
                                type='tel'
                                name='phoneNumber'
                                placeholder='XXXXXXXXX'
                                maxLength={20}
                                defaultValue={orderData.phoneNumber}
                            />
                        </Field>
                        {orderData.orderType === 'Delivery' ? (
                            <>
                                <Field>
                                    <FieldLabel htmlFor='city'>City</FieldLabel>
                                    <Input
                                        id='city'
                                        type='text'
                                        name='city'
                                        placeholder='City'
                                        maxLength={50}
                                        defaultValue={orderData.delivery?.city}
                                    />
                                </Field>
                                <Field>
                                    <FieldLabel htmlFor='address'>
                                        Address
                                    </FieldLabel>
                                    <Input
                                        id='address'
                                        type='text'
                                        name='address'
                                        placeholder='Address'
                                        maxLength={255}
                                        defaultValue={orderData.delivery?.address}
                                    />
                                </Field>
                            </>
                        ) : null}
                        <Field>
                            <div className='flex flex-row justify-start items-center gap-2'>
                                <FieldLabel htmlFor='isPaid'>
                                    Is paid?
                                </FieldLabel>
                                <Input
                                    id='isPaid'
                                    type='checkbox'
                                    name='isPaid'
                                    className='size-4'
                                    defaultChecked={orderData.isPaid}
                                />
                            </div>
                        </Field>
                        {error ? (
                            <p className='text-destructive'>{error}</p>
                        ) : null}
                        <FieldGroup>
                            <Field>
                                <Button type='submit' disabled={submitting}>
                                    Submit
                                </Button>
                            </Field>
                        </FieldGroup>
                    </FieldGroup>
                </form>
            </CardContent>
        </Card>
    );
}
