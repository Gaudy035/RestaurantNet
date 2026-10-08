'use client'

import MenuItem from "@/interfaces/MenuItem";
import {useEffect, useState} from "react";
import Category from "@/interfaces/Category";
import {adminApiFetch} from "@/lib/api";
import {getErrorMessage} from "@/lib/api-error";
import {Spinner} from "@/components/ui/spinner";
import {Card, CardAction, CardDescription, CardHeader, CardTitle} from "@/components/ui/card";
import {buttonVariants} from "@/components/ui/button";
import {useEmployee} from "@/lib/employee-context";
import {Badge} from "@/components/ui/badge";
import {cn} from "@/lib/utils";
import Link from "next/link";

export default function MenuItemInfo({ itemId }: { itemId: string }){
    const [itemData, setItemData] = useState<MenuItem | null>(null);
    const [categoryData, setCategoryData] = useState<Category | null>(null);
    const [itemLoading, setItemLoading] = useState<boolean>(true);
    const [categoryLoading, setCategoryLoading] = useState<boolean>(true);
    const [itemError, setItemError] = useState<string | null>(null);
    const [categoryError, setCategoryError] = useState<string | null>(null);

    const employee = useEmployee()
    const isAdmin = employee.employeeData?.isAdmin;

    useEffect(() => {
        adminApiFetch<MenuItem>(`/admin/menuitems/${itemId}`, {
            method: 'GET',
            cache: 'no-store'
        })
            .then((data) => setItemData(data))
            .catch((error) => setItemError(getErrorMessage(error)))
            .finally(() => setItemLoading(false));
    }, [itemId]);

    useEffect(() => {
        adminApiFetch<Category>(`/admin/categories/${itemData?.categoryId}`, {
            method: 'GET',
            cache: 'no-store'
        })
            .then((data) => {
                setCategoryData(data);
                setCategoryError(null);
            })
            .catch((error) => itemData && setCategoryError(getErrorMessage(error)))
            .finally(() => setCategoryLoading(false));
    }, [itemData])

    return (
        <div className='flex justify-center flex-col items-center'>
            <p className='text-destructive'>{itemError ? itemError : null}</p>
            {itemLoading ? (
                <Spinner className='size-8' />
            ) : itemData ? (
                <Card className='flex w-full py-6 px-3'>
                    <CardHeader className='flex flex-row justify-between items-center'>
                        <div className='flex flex-col justify-center items-start gap-2'>
                            <CardTitle className='flex justify-center items-center flex-row gap-4 text-2xl font-semibold'>
                                {itemData.name}
                                {itemData.isPinned && <Badge>Pinned</Badge>}
                                {isAdmin && <Badge>{itemData.isAvailable ? 'Available' : 'Unavailable'}</Badge> }
                            </CardTitle>
                            <CardDescription className='flex flex-col justify-center items-start gap-4 text-lg'>
                                <p>Item ID: {itemData.itemId}</p>
                                <p>Price {itemData.price.toFixed(2)}</p>
                                {
                                    categoryLoading ? <p>Loading category... </p>
                                    : categoryError ? <p className='text-destructive'>{categoryError}</p>
                                    : categoryData && <p>Category: {categoryData.categoryName}</p>
                                }
                            </CardDescription>
                        </div>

                        <CardAction>
                            <Link
                                href={`/admin/menuitems/${itemId}/update`}
                                className={cn(
                                    buttonVariants({ variant: 'default', size: 'default' }),
                                )}
                            >
                                Modify menu item
                            </Link>
                        </CardAction>
                    </CardHeader>
                </Card>
            ) : null}
        </div>
    );
}