'use client';

import { useState } from 'react';
import { format } from 'date-fns';
import { CalendarIcon } from 'lucide-react';
import { cn } from '@/lib/utils';
import { Button } from '@/components/ui/button';
import { Calendar } from '@/components/ui/calendar';
import {
    Popover,
    PopoverContent,
    PopoverTrigger,
} from '@/components/ui/popover';

export default function OrdersDatePicker({
    date,
    onDateChange,
}: {
    date: Date | undefined;
    onDateChange: (date: Date | undefined) => void;
}) {
    const [open, setOpen] = useState<boolean>(false);

    return (
        <Popover open={open} onOpenChange={setOpen}>
            <PopoverTrigger
                render={
                    <Button
                        variant='outline'
                        className={cn(
                            'justify-start text-left font-normal',
                            !date && 'text-muted-foreground',
                        )}
                    >
                        <CalendarIcon />
                        {date ? format(date, 'dd.MM.yyyy') : 'Pick a date'}
                    </Button>
                }
            />
            <PopoverContent className='w-auto p-0' align='start'>
                <Calendar
                    mode='single'
                    selected={date}
                    onSelect={(selected) => {
                        onDateChange(selected);
                        setOpen(false);
                    }}
                />
                {date ? (
                    <div className='border-t p-2'>
                        <Button
                            variant='ghost'
                            size='sm'
                            className='w-full'
                            onClick={() => {
                                onDateChange(undefined);
                                setOpen(false);
                            }}
                        >
                            Clear date
                        </Button>
                    </div>
                ) : null}
            </PopoverContent>
        </Popover>
    );
}
