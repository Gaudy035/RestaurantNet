'use client'

import {useRouter} from "next/navigation";
import {Input} from "@/components/ui/input";
import {Button} from "@/components/ui/button";
import {Search} from "lucide-react";
import React, {useState} from "react";

export default function AdminMenuItemsSearchBar({ val }: { val?: string }) {
    const router = useRouter();
    const [menuItemData, setMenuItemData] = useState<string>(val ?? '');

    const onSubmit = (e:React.SyntheticEvent<HTMLFormElement>) => {
        e.preventDefault();

        if (menuItemData) {
            setMenuItemData(menuItemData.trim());
            router.push(`/admin/menuitems?param=${encodeURIComponent(menuItemData)}`);
        } else {
            router.push(`/admin/menuitems`);
        }
    }

    return (
        <form
            onSubmit={onSubmit}
            className='flex items-center justify-center gap-2'
        >
            <Input
                placeholder='Enter menu item name...'
                type='text'
                onChange={(e) => setMenuItemData(e.target.value)}
                value={menuItemData}
            />
            <Button size='icon' type='submit'>
                <Search />
            </Button>
        </form>
    );
}