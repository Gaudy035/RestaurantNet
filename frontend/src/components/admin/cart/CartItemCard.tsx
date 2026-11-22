'use client';

import CartItem from '@/interfaces/CartItem';
import {
    Card,
    CardAction,
    CardDescription,
    CardHeader,
    CardTitle,
} from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Minus, Plus } from 'lucide-react';

export default function CartItemCard({
    itemData,
    pending,
    onIncrease,
    onDecrease,
}: {
    itemData: CartItem;
    pending: boolean;
    onIncrease: (itemId: string) => void | Promise<void>;
    onDecrease: (itemId: string) => void | Promise<void>;
}) {
    return (
        <Card className='flex w-full px-2 py-5'>
            <CardHeader className='flex flex-row justify-between items-center'>
                <div className='flex flex-col justify-center items-start gap-2'>
                    <CardTitle>{itemData.name}</CardTitle>
                    <CardDescription className='flex flex-row justify-center items-center gap-4'>
                        <p>Price: {itemData.price.toFixed(2)}</p>
                        <p>
                            Subtotal:{' '}
                            {(itemData.price * itemData.quantity).toFixed(2)}
                        </p>
                    </CardDescription>
                </div>

                <CardAction className='flex flex-row justify-center items-center gap-2'>
                    <Button
                        variant='outline'
                        size='icon'
                        aria-label={`Remove one ${itemData.name} from cart`}
                        disabled={pending}
                        onClick={() => onDecrease(itemData.itemId)}
                    >
                        <Minus />
                    </Button>
                    <p className='w-8 text-center'>{itemData.quantity}</p>
                    <Button
                        variant='outline'
                        size='icon'
                        aria-label={`Add one ${itemData.name} to cart`}
                        disabled={pending}
                        onClick={() => onIncrease(itemData.itemId)}
                    >
                        <Plus />
                    </Button>
                </CardAction>
            </CardHeader>
        </Card>
    );
}
