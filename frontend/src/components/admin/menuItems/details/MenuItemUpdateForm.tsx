'use client'

import {adminApiFetch} from "@/lib/api";
import {Card, CardContent, CardDescription, CardHeader, CardTitle} from "@/components/ui/card";
import {Field, FieldGroup, FieldLabel} from "@/components/ui/field";
import {Input} from "@/components/ui/input";
import {Button} from "@/components/ui/button";
import {
    Select,
    SelectContent,
    SelectGroup,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/select";
import {useEffect, useState} from "react";
import {useRouter} from "next/navigation";
import Category from "@/interfaces/Category";
import MenuItem from "@/interfaces/MenuItem";
import {getErrorMessage} from "@/lib/api-error";
import {Spinner} from "@/components/ui/spinner";
import {toast} from "sonner";

export default function MenuItemUpdateForm({ itemId }: { itemId: string }) {
    const router = useRouter();
    const [error, setError] = useState<string | null>(null);
    const [loading, setLoading] = useState(true);
    const [categoryLoading, setCategoryLoading] = useState<boolean>(true);
    const [itemData, setItemData] = useState<MenuItem | null>(null);
    const [categories, setCategories] = useState<Category[]>([]);
    const [categoryId, setCategoryId] = useState<string | null>(null);

    useEffect(() => {
        adminApiFetch<MenuItem>(`/admin/menuitems/${itemId}`, {
            method: "GET",
            cache: "no-store"
        })
            .then((data) => {
                setItemData(data);
                setCategoryId(String(data.categoryId));
            })
            .catch((error) => setError(getErrorMessage(error)))
            .finally(() => setLoading(false));
    }, [itemId])

    useEffect(() => {
        adminApiFetch<Category[]>("/admin/categories", {
            method: "GET",
            cache: "no-store"
        })
            .then((data) => setCategories(data))
            .catch((error) => setError(getErrorMessage(error)))
            .finally(() => setCategoryLoading(false));
    }, []);

    const onSubmit = async (e: React.SyntheticEvent<HTMLFormElement>) => {
        e.preventDefault();

        const formData = new FormData(e.currentTarget);

        const payload: {
            price?: number;
            categoryId?: number;
            isAvailable?: boolean;
            isPinned?: boolean;
        } = {};

        const rawPrice = formData.get('price');

        if(typeof rawPrice === 'string' && rawPrice.trim() !== '') {
            const newPrice = Number(rawPrice);

            if (Number.isFinite(newPrice) && newPrice >= 0 && newPrice <= 10000 && newPrice !== itemData?.price) {
                payload.price = newPrice;
            }
        }

        if (categoryId && categoryId !== String(itemData?.categoryId)) {
            payload.categoryId = Number(categoryId);
        }

        const isAvailable = formData.get('isAvailable') === 'on';
        if (isAvailable !== itemData?.isAvailable) {
            payload.isAvailable = isAvailable;
        }

        const isPinned = formData.get('isPinned') === 'on';
        if (isPinned !== itemData?.isPinned) {
            payload.isPinned = isPinned;
        }

        try {
            await adminApiFetch(`/admin/menuitems/${itemId}`, {
                method: 'PATCH',
                body: JSON.stringify(payload),
            })

            setError(null);
            toast.success("Item updated successfully");
            router.push(`/admin/menuitems/${itemId}`);
        } catch (error) {
            setError(getErrorMessage(error));
        }
    }

    if (loading || categoryLoading) {
        return <Spinner className='size-8' />;
    }

    return (
        <Card>
            <CardHeader>
                <CardTitle>Update a menu item</CardTitle>
                <CardDescription>Enter new data for item {"name here"}</CardDescription>
            </CardHeader>
            <CardContent>
                <form onSubmit={onSubmit}>
                    <FieldGroup>
                        <Field>
                            <FieldLabel htmlFor='price'>Price</FieldLabel>
                            <Input
                                id='price'
                                type='number'
                                step='0.01'
                                min='0.00'
                                max='10000'
                                inputMode='decimal'
                                name='price'
                                placeholder='0.00'
                                defaultValue={itemData?.price.toFixed(2)}
                                required
                            />
                        </Field>
                        <Field>
                            <FieldLabel htmlFor='categoryId'>Category</FieldLabel>
                            <Select
                                items={categories.map((c) => ({
                                    value: String(c.categoryId),
                                    label: c.categoryName,
                                }))}
                                value={categoryId}
                                onValueChange={(value) => setCategoryId(value)}
                            >
                                <SelectTrigger id='categoryId' className='w-full'>
                                    <SelectValue placeholder='Select category' />
                                </SelectTrigger>
                                <SelectContent>
                                    <SelectGroup>
                                        {categories.map((c) => (
                                            <SelectItem
                                                key={c.categoryId}
                                                value={String(c.categoryId)}
                                            >
                                                {c.categoryName}
                                            </SelectItem>
                                        ))}
                                    </SelectGroup>
                                </SelectContent>
                            </Select>
                        </Field>
                        <Field>
                            <div className='flex flex-row justify-start items-center gap-2'>
                                <FieldLabel htmlFor='isAvailable'>Is available?</FieldLabel>
                                <Input
                                    id='isAvailable'
                                    type='checkbox'
                                    name='isAvailable'
                                    className='size-4'
                                    defaultChecked={itemData?.isAvailable}
                                />
                            </div>
                        </Field>
                        <Field>
                            <div className='flex flex-row justify-start items-center gap-2'>
                                <FieldLabel htmlFor='isPinned'>Is pinned?</FieldLabel>
                                <Input
                                    id='isPinned'
                                    type='checkbox'
                                    name='isPinned'
                                    className='size-4'
                                    defaultChecked={itemData?.isPinned}
                                />
                            </div>
                        </Field>
                        {error ? <p className='text-destructive'>{error}</p> : null}
                        <FieldGroup>
                            <Field>
                                <Button type='submit'>Submit</Button>
                            </Field>
                        </FieldGroup>
                    </FieldGroup>
                </form>
            </CardContent>
        </Card>
    );
}