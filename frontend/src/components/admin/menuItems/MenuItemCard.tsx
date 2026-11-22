'use client'

import MenuItem from "@/interfaces/MenuItem";
import {useRouter} from "next/navigation";
import {Card, CardAction, CardDescription, CardHeader, CardTitle} from "@/components/ui/card";
import {Badge} from "@/components/ui/badge";
import {Button} from "@/components/ui/button";
import {useEmployee} from "@/lib/employee-context";
import {adminApiFetch} from "@/lib/api";
import {getErrorMessage} from "@/lib/api-error";
import {toast} from "sonner";
import {Loader2, ShoppingCart} from "lucide-react";
import React, {useState} from "react";

export default function MenuItemCard({ itemData }: { itemData: MenuItem }) {
    const router = useRouter();
    const employee = useEmployee();
    const isAdmin = employee.employeeData?.isAdmin;
    const [adding, setAdding] = useState<boolean>(false);

    const addToCart = async (e: React.MouseEvent<HTMLButtonElement>) => {
        e.stopPropagation();
        setAdding(true);

        try {
            await adminApiFetch(`/admin/orders/cart/${itemData.itemId}`, {
                method: 'POST',
            });
            toast.success(`${itemData.name} added to cart`);
        } catch (error) {
            toast.error(getErrorMessage(error));
        } finally {
            setAdding(false);
        }
    };

    return (
        <Card
            className='group flex w-full px-2 py-5 cursor-pointer hover:bg-accent hover:text-accent-foreground transition'
            onClick={() => router.push(`/admin/menuitems/${itemData.itemId}`)}
        >
            <CardHeader className='flex flex-row justify-between items-center'>
                <div className='flex flex-col justify-center items-start gap-2'>
                    <CardTitle className='flex justify-center items-center flex-row gap-2'>
                        {itemData.name}
                        {isAdmin && <Badge>{itemData.isAvailable ? 'Available' : 'Unavailable'}</Badge>}
                    </CardTitle>
                    <CardDescription className='flex flex-row justify-center items-center gap-4'>
                        <p>Price: {(itemData.price.toFixed(2))}</p>
                    </CardDescription>
                </div>

                <CardAction className='flex flex-row justify-center items-center gap-2'>
                    <Button
                        variant='outline'
                        size='icon'
                        aria-label={`Add ${itemData.name} to cart`}
                        disabled={adding || !itemData.isAvailable}
                        onClick={addToCart}
                    >
                        {adding ? <Loader2 className='animate-spin' /> : <ShoppingCart />}
                    </Button>
                    <Button
                        variant='link'
                        size={'lg'}
                        className='pointer-events-none group-hover:underline'
                    >
                        Details
                    </Button>
                </CardAction>
            </CardHeader>
        </Card>
    );
}