'use client';

import React, { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { toast } from 'sonner';
import { adminApiFetch } from '@/lib/api';
import { getErrorMessage } from '@/lib/api-error';
import Cart from '@/interfaces/Cart';
import ClientData from '@/interfaces/ClientData';
import LocationData from '@/interfaces/LocationData';
import Order from '@/interfaces/Order';
import OrderType from '@/types/order-type';
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

const orderTypes: { value: OrderType; label: string }[] = [
    { value: 'DineIn', label: 'Dine in' },
    { value: 'Pickup', label: 'Pickup' },
    { value: 'Delivery', label: 'Delivery' },
];

const paymentMethods: { value: PaymentMethod; label: string }[] = [
    { value: 'Card', label: 'Card' },
    { value: 'Cash', label: 'Cash' },
];

export default function AdminCartFinalizeForm() {
    const router = useRouter();
    const [locations, setLocations] = useState<LocationData[]>([]);
    const [clients, setClients] = useState<ClientData[]>([]);
    const [cart, setCart] = useState<Cart | null>(null);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);
    const [submitting, setSubmitting] = useState<boolean>(false);

    const [locationId, setLocationId] = useState<string | null>(null);
    const [clientId, setClientId] = useState<string>('none');
    const [orderType, setOrderType] = useState<OrderType>('DineIn');
    const [paymentMethod, setPaymentMethod] = useState<PaymentMethod | null>(
        null,
    );

    useEffect(() => {
        Promise.all([
            adminApiFetch<LocationData[]>('/admin/locations', {
                method: 'GET',
                cache: 'no-store',
            }),
            adminApiFetch<ClientData[]>('/admin/users/clients', {
                method: 'GET',
                cache: 'no-store',
            }),
            adminApiFetch<Cart>('/admin/orders/cart', {
                method: 'GET',
                cache: 'no-store',
            }),
        ])
            .then(([locationsData, clientsData, cartData]) => {
                setError(null);
                setLocations(locationsData);
                setClients(clientsData);
                setCart(cartData);
            })
            .catch((e) => setError(getErrorMessage(e)))
            .finally(() => setLoading(false));
    }, []);

    const onSubmit = async (e: React.SyntheticEvent<HTMLFormElement>) => {
        e.preventDefault();
        setError(null);

        if (!locationId || !paymentMethod) {
            setError('Select location and payment method');
            return;
        }

        const formData = new FormData(e.currentTarget);
        const phoneNumber =
            formData.get('phoneNumber')?.toString().trim() ?? '';
        const city = formData.get('city')?.toString().trim() ?? '';
        const address = formData.get('address')?.toString().trim() ?? '';
        const isPaid = formData.get('isPaid') === 'on';

        if (orderType === 'Delivery' && (!phoneNumber || !city || !address)) {
            setError('Enter phone number, city and address for delivery');
            return;
        }

        const payload = {
            locationId: Number(locationId),
            clientId: clientId !== 'none' ? Number(clientId) : undefined,
            isPaid: isPaid,
            paymentMethod: paymentMethod,
            orderType: orderType,
            phoneNumber: phoneNumber,
            ...(orderType === 'Delivery' ? { city: city, address: address } : {}),
        };

        setSubmitting(true);

        try {
            const createdOrder = await adminApiFetch<Order>('/admin/orders', {
                method: 'POST',
                body: JSON.stringify(payload),
            });

            toast.success('Order finalized successfully');
            router.push(`/admin/orders/${createdOrder.orderId}`);
        } catch (err) {
            setError(getErrorMessage(err));
        } finally {
            setSubmitting(false);
        }
    };

    if (loading) {
        return <Spinner className='size-8' />;
    }

    if (cart && cart.items.length === 0) {
        return (
            <Card>
                <CardHeader>
                    <CardTitle>Cart is empty</CardTitle>
                    <CardDescription>
                        Add items to the cart before finalizing an order.
                    </CardDescription>
                </CardHeader>
            </Card>
        );
    }

    return (
        <Card>
            <CardHeader>
                <CardTitle>Finalize an order</CardTitle>
                <CardDescription>
                    {cart
                        ? `${cart.items.length} item(s), total: ${cart.totalPrice.toFixed(2)}`
                        : 'Enter order details below'}
                </CardDescription>
            </CardHeader>
            <CardContent>
                <form onSubmit={onSubmit}>
                    <FieldGroup>
                        <Field>
                            <FieldLabel htmlFor='locationId'>Location</FieldLabel>
                            <Select
                                items={locations.map((l) => ({
                                    value: String(l.locationId),
                                    label: `${l.city}, ${l.address}`,
                                }))}
                                value={locationId}
                                onValueChange={(value) => setLocationId(value)}
                            >
                                <SelectTrigger
                                    id='locationId'
                                    className='w-full'
                                >
                                    <SelectValue placeholder='Select location' />
                                </SelectTrigger>
                                <SelectContent>
                                    <SelectGroup>
                                        {locations.map((l) => (
                                            <SelectItem
                                                key={l.locationId}
                                                value={String(l.locationId)}
                                            >
                                                {l.city}, {l.address}
                                            </SelectItem>
                                        ))}
                                    </SelectGroup>
                                </SelectContent>
                            </Select>
                        </Field>
                        <Field>
                            <FieldLabel htmlFor='clientId'>Client</FieldLabel>
                            <Select
                                items={[
                                    {
                                        value: 'none',
                                        label: 'No client (walk-in)',
                                    },
                                    ...clients.map((c) => ({
                                        value: String(c.userId),
                                        label: `${c.firstName} ${c.lastName}`,
                                    })),
                                ]}
                                value={clientId}
                                onValueChange={(value) =>
                                    setClientId(value ?? 'none')
                                }
                            >
                                <SelectTrigger id='clientId' className='w-full'>
                                    <SelectValue placeholder='Select client' />
                                </SelectTrigger>
                                <SelectContent>
                                    <SelectGroup>
                                        <SelectItem value='none'>
                                            No client (walk-in)
                                        </SelectItem>
                                        {clients.map((c) => (
                                            <SelectItem
                                                key={c.userId}
                                                value={String(c.userId)}
                                            >
                                                {c.firstName} {c.lastName}
                                            </SelectItem>
                                        ))}
                                    </SelectGroup>
                                </SelectContent>
                            </Select>
                        </Field>
                        <Field>
                            <FieldLabel htmlFor='orderType'>Order type</FieldLabel>
                            <Select
                                items={orderTypes}
                                value={orderType}
                                onValueChange={(value) =>
                                    value && setOrderType(value as OrderType)
                                }
                            >
                                <SelectTrigger
                                    id='orderType'
                                    className='w-full'
                                >
                                    <SelectValue placeholder='Select order type' />
                                </SelectTrigger>
                                <SelectContent>
                                    <SelectGroup>
                                        {orderTypes.map((ot) => (
                                            <SelectItem
                                                key={ot.value}
                                                value={ot.value}
                                            >
                                                {ot.label}
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
                                required={orderType === 'Delivery'}
                            />
                        </Field>
                        {orderType === 'Delivery' ? (
                            <>
                                <Field>
                                    <FieldLabel htmlFor='city'>City</FieldLabel>
                                    <Input
                                        id='city'
                                        type='text'
                                        name='city'
                                        placeholder='City'
                                        maxLength={50}
                                        required
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
                                        required
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
                                />
                            </div>
                        </Field>
                        {error ? (
                            <p className='text-destructive'>{error}</p>
                        ) : null}
                        <FieldGroup>
                            <Field>
                                <Button type='submit' disabled={submitting}>
                                    Finalize order
                                </Button>
                            </Field>
                        </FieldGroup>
                    </FieldGroup>
                </form>
            </CardContent>
        </Card>
    );
}
