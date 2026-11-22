'use client';

import React, { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { format, parseISO } from 'date-fns';
import { Filter } from 'lucide-react';
import { adminApiFetch } from '@/lib/api';
import { getErrorMessage } from '@/lib/api-error';
import LocationData from '@/interfaces/LocationData';
import { Button } from '@/components/ui/button';
import {
    Select,
    SelectContent,
    SelectGroup,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from '@/components/ui/select';
import OrdersDatePicker from '@/components/admin/orders/OrdersDatePicker';

const parseDate = (value?: string) => {
    if (!value) {
        return undefined;
    }

    const date = parseISO(value);

    return isNaN(date.getTime()) ? undefined : date;
};

export default function AdminOrdersFilterBar({
    date,
    locationId,
}: {
    date?: string;
    locationId?: string;
}) {
    const router = useRouter();
    const [selectedDate, setSelectedDate] = useState<Date | undefined>(
        parseDate(date),
    );
    const [selectedLocationId, setSelectedLocationId] = useState<string>(
        locationId ?? 'all',
    );
    const [locations, setLocations] = useState<LocationData[]>([]);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        adminApiFetch<LocationData[]>('/admin/locations', {
            method: 'GET',
            cache: 'no-store',
        })
            .then((data) => setLocations(data))
            .catch((e) => setError(getErrorMessage(e)));
    }, []);

    const onSubmit = (e: React.SyntheticEvent<HTMLFormElement>) => {
        e.preventDefault();

        const params = new URLSearchParams();

        if (selectedDate) {
            params.set('date', format(selectedDate, 'yyyy-MM-dd'));
        }

        if (selectedLocationId !== 'all') {
            params.set('locationId', selectedLocationId);
        }

        const query = params.toString();
        router.push(`/admin/orders${query ? `?${query}` : ''}`);
    };

    return (
        <form
            onSubmit={onSubmit}
            className='flex items-center justify-center gap-2'
        >
            <OrdersDatePicker
                date={selectedDate}
                onDateChange={setSelectedDate}
            />
            <Select
                items={[
                    { value: 'all', label: 'All locations' },
                    ...locations.map((l) => ({
                        value: String(l.locationId),
                        label: `${l.city}, ${l.address}`,
                    })),
                ]}
                value={selectedLocationId}
                onValueChange={(value) =>
                    setSelectedLocationId(value ?? 'all')
                }
            >
                <SelectTrigger className='w-[220px]'>
                    <SelectValue placeholder='All locations' />
                </SelectTrigger>
                <SelectContent>
                    <SelectGroup>
                        <SelectItem value='all'>All locations</SelectItem>
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
            <Button size='icon' type='submit' aria-label='Filter orders'>
                <Filter />
            </Button>
            {error ? (
                <p className='text-sm text-destructive'>{error}</p>
            ) : null}
        </form>
    );
}
