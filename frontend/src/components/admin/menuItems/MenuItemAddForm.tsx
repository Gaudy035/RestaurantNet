'use client'

import React, {useEffect, useState} from "react";
import {adminApiFetch} from "@/lib/api";
import {toast} from "sonner";
import {getErrorMessage} from "@/lib/api-error";
import {Card, CardContent, CardDescription, CardHeader, CardTitle} from "@/components/ui/card";
import {Field, FieldGroup, FieldLabel} from "@/components/ui/field";
import {Input} from "@/components/ui/input";
import {Button} from "@/components/ui/button";
import {useRouter} from "next/navigation";
import Category from "@/interfaces/Category";

export default function AdminMenuItemAddForm() {
    const router = useRouter();
    const [error, setError] = React.useState<string | null>(null);
    const [categories, setCategories] = useState<Category[]>([]);
    const [categoryId, setCategoryId] = useState<string | null>(null);

    useEffect(() => {
        adminApiFetch<Category[]>('/admin/categories', {
            method: 'GET',
            cache: 'no-store'
        })
            .then((data) => setCategories(data))
            .catch((error) => setError(getErrorMessage(error)))
    }, []);

    const onSubmit = async (e: React.SyntheticEvent<HTMLFormElement>) => {
        e.preventDefault();
        setError(null);

        const formData = new FormData(e.currentTarget);
        const payload = {
            ...Object.fromEntries(formData.entries()),
            isAvailable: formData.get('isAvailable') === 'on',
            isPinned: formData.get('isPinned') === 'on',
        };

        try {
            await adminApiFetch('/admin/menuitems', {
                method: 'POST',
                body: JSON.stringify(payload),
            });

            setError(null);
            toast.success('Menu item created');
            router.push('/admin/menuitems');
        } catch (err) {
            setError(getErrorMessage(err));
        }
    };

    return (
        <Card>
            <CardHeader>
                <CardTitle>Create a menu item</CardTitle>
                <CardDescription>Enter new menu item data below</CardDescription>
            </CardHeader>
            <CardContent>
                <form onSubmit={onSubmit}>
                    <FieldGroup>
                        <Field>
                            <FieldLabel htmlFor='name'>Name</FieldLabel>
                            <Input
                                id='name'
                                name='name'
                                type='text'
                                placeholder='Name'
                                required
                            />
                        </Field>
                        <Field>
                            <FieldLabel htmlFor='price'>Price</FieldLabel>
                            <Input
                                id='price'
                                type='number'
                                step='0.01'
                                min='0.00'
                                inputMode='decimal'
                                name='price'
                                placeholder='0.00'
                                required
                            />
                        </Field>
                        <Field>
                            <FieldLabel htmlFor='lastName'>Category</FieldLabel>
                            <select
                                onChange={(e) => setCategoryId(e.target.value)}
                                className={
                                    categoryId ? 'text-foreground' : 'text-muted-foreground'
                                }
                                name='categoryId'
                                id='categoryId'
                                defaultValue=''
                                required
                            >
                                <option value='' disabled>
                                    Select category
                                </option>
                                {categories.map((c) => (
                                    <option key={c.categoryId} value={c.categoryId}>
                                        {c.categoryName}
                                    </option>
                                ))}
                            </select>
                        </Field>
                        <Field>
                            <div className='flex flex-row justify-start items-center gap-2'>
                                <FieldLabel htmlFor='isAvailable'>Is available?</FieldLabel>
                                <Input
                                    id='isAvailable'
                                    type='checkbox'
                                    name='isAvailable'
                                    className='size-4'
                                />
                            </div>
                        </Field><Field>
                            <div className='flex flex-row justify-start items-center gap-2'>
                                <FieldLabel htmlFor='isPinned'>Is pinned?</FieldLabel>
                                <Input
                                    id='isPinned'
                                    type='checkbox'
                                    name='isPinned'
                                    className='size-4'
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