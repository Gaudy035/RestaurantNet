'use client';

import { useEffect, useState } from 'react';
import Link from 'next/link';
import { toast } from 'sonner';
import { adminApiFetch } from '@/lib/api';
import { getErrorMessage } from '@/lib/api-error';
import { cn } from '@/lib/utils';
import Cart from '@/interfaces/Cart';
import CartItem from '@/interfaces/CartItem';
import {
    Card,
    CardAction,
    CardHeader,
    CardTitle,
} from '@/components/ui/card';
import { Button, buttonVariants } from '@/components/ui/button';
import { Spinner } from '@/components/ui/spinner';
import ConfirmDialog from '@/components/admin/AdminConfirmDialog';
import CartItemCard from '@/components/admin/cart/CartItemCard';

export default function AdminCartMain() {
    const [cart, setCart] = useState<Cart | null>(null);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);
    const [pendingItemId, setPendingItemId] = useState<string | null>(null);
    const [clearing, setClearing] = useState<boolean>(false);

    useEffect(() => {
        adminApiFetch<Cart>('/admin/orders/cart', {
            method: 'GET',
            cache: 'no-store',
        })
            .then((data) => {
                setError(null);
                setCart(data);
            })
            .catch((e) => setError(getErrorMessage(e)))
            .finally(() => setLoading(false));
    }, []);

    const setCartItem = (item: CartItem | null, itemId: string) => {
        setCart((prev) => {
            if (!prev) {
                return prev;
            }

            const items = item
                ? prev.items.map((i) => (i.itemId === itemId ? item : i))
                : prev.items.filter((i) => i.itemId !== itemId);

            return {
                items,
                totalPrice: items.reduce(
                    (total, i) => total + i.price * i.quantity,
                    0,
                ),
            };
        });
    };

    const increaseQuantity = async (itemId: string) => {
        setError(null);
        setPendingItemId(itemId);

        try {
            const updatedItem = await adminApiFetch<CartItem>(
                `/admin/orders/cart/${itemId}`,
                { method: 'POST' },
            );
            setCartItem(updatedItem, itemId);
        } catch (e) {
            setError(getErrorMessage(e));
        } finally {
            setPendingItemId(null);
        }
    };

    const decreaseQuantity = async (itemId: string) => {
        setError(null);
        setPendingItemId(itemId);

        try {
            const updatedItem = await adminApiFetch<CartItem | null>(
                `/admin/orders/cart/${itemId}`,
                { method: 'DELETE' },
            );
            setCartItem(updatedItem, itemId);
        } catch (e) {
            setError(getErrorMessage(e));
        } finally {
            setPendingItemId(null);
        }
    };

    const clearCart = async () => {
        setError(null);
        setClearing(true);

        try {
            await adminApiFetch('/admin/orders/cart', { method: 'DELETE' });
            setCart({ items: [], totalPrice: 0 });
            toast.success('Cart cleared successfully');
        } catch (e) {
            setError(getErrorMessage(e));
        } finally {
            setClearing(false);
        }
    };

    return (
        <div className='flex justify-center flex-col items-center'>
            <p className='text-destructive'>{error ? error : null}</p>
            {loading ? (
                <Spinner className='size-8' />
            ) : cart && cart.items.length > 0 ? (
                <div className='flex flex-col m-8 w-full justify-center items-center gap-4'>
                    {cart.items.map((ci) => (
                        <CartItemCard
                            key={ci.itemId}
                            itemData={ci}
                            pending={pendingItemId === ci.itemId}
                            onIncrease={increaseQuantity}
                            onDecrease={decreaseQuantity}
                        />
                    ))}
                    <Card className='flex w-full px-2 py-5'>
                        <CardHeader className='flex flex-row justify-between items-center'>
                            <CardTitle className='text-lg'>
                                Total: {cart.totalPrice.toFixed(2)}
                            </CardTitle>
                            <CardAction className='flex flex-row justify-center items-center gap-2'>
                                <ConfirmDialog
                                    trigger={
                                        <Button
                                            variant='destructive'
                                            disabled={clearing}
                                        >
                                            Empty cart
                                        </Button>
                                    }
                                    variant='destructive'
                                    title='Empty cart'
                                    description='Remove all items from your cart?'
                                    onConfirm={() => clearCart()}
                                />
                                <Link
                                    href='/admin/cart/finalize'
                                    className={cn(
                                        buttonVariants({
                                            variant: 'default',
                                            size: 'default',
                                        }),
                                    )}
                                >
                                    Finalize order
                                </Link>
                            </CardAction>
                        </CardHeader>
                    </Card>
                </div>
            ) : (
                <p>Your cart is empty</p>
            )}
        </div>
    );
}
