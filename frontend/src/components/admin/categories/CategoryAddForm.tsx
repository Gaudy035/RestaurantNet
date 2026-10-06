'use client'

import {Card, CardContent, CardDescription, CardHeader, CardTitle} from "@/components/ui/card";
import {Field, FieldError, FieldGroup, FieldLabel} from "@/components/ui/field";
import {Input} from "@/components/ui/input";
import {Button} from "@/components/ui/button";
import React from "react";
import {useRouter} from "next/navigation";
import {adminApiFetch} from "@/lib/api";
import {toast} from "sonner";
import {getErrorMessage, isApiError} from "@/lib/api-error";

export default function AdminCategoryAddForm() {
    const router = useRouter();
    const [nameError, setNameError] = React.useState<string | null>(null);
    const [error, setError] = React.useState<string | null>(null);

    const onSubmit = async (e: React.SyntheticEvent<HTMLFormElement>) => {
        e.preventDefault();
        setNameError(null);
        setError(null);

        const formData = new FormData(e.currentTarget);
        const payload = Object.fromEntries(formData.entries());

        try {
            await adminApiFetch('/admin/categories', {
                method: 'POST',
                body: JSON.stringify(payload),
            });

            setError(null);
            toast.success("Category added successfully.");
            router.push("/admin/categories");
        } catch (err) {
            if (isApiError(err) && err.code === 'CategoryAlreadyExists') {
                setNameError(getErrorMessage(err));
            } else {
                setError(getErrorMessage(err));
            }
        }
    }

    return (
        <Card>
            <CardHeader>
                <CardTitle>Create a new category</CardTitle>
                <CardDescription>Enter new category name below</CardDescription>
            </CardHeader>
            <CardContent>
                <form onSubmit={onSubmit}>
                    <FieldGroup>
                        <Field>
                            <FieldLabel htmlFor='categoryName'>Category name</FieldLabel>
                            <Input
                                id='categoryName'
                                name='categoryName'
                                type='text'
                                placeholder='Category name'
                                aria-invalid={nameError !== null}
                                onChange={() => setNameError(null)}
                                required
                            />
                            {nameError ? <FieldError>{nameError}</FieldError> : null}
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