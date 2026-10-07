'use client'

import {Spinner} from "@/components/ui/spinner";
import MenuItemCard from "@/components/admin/menuItems/MenuItemCard";
import {useEffect, useState} from "react";
import MenuItem from "@/interfaces/MenuItem";
import {adminApiFetch} from "@/lib/api";
import {getErrorMessage} from "@/lib/api-error";

export default function AdminMenuItemsMain({ itemData }: { itemData?: string }) {
    const [menuItems, setMenuItems] = useState<MenuItem[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        const endpoint = itemData
            ? `/admin/menuitems?param=${encodeURIComponent(itemData)}`
            : '/admin/menuitems'

        adminApiFetch<MenuItem[]>(endpoint, {
            method: "GET",
            cache: "no-store"
        })
            .then((data) => setMenuItems(data))
            .catch((error) => setError(getErrorMessage(error)))
            .finally(() => setLoading(false));
    }, [itemData]);

    return (
        <div className='flex justify-center flex-col items-center'>
            <p className='text-destructive'>{error ? error : null}</p>
            {loading ? (
                <Spinner className='size-8' />
            ) : menuItems.length > 0 ? (
                <div className='flex flex-col m-8 w-full justify-center items-center gap-4'>
                    {menuItems.map((mi) => (
                        // <LocationCard key={l.locationId} locationData={l} />
                        <MenuItemCard key={mi.itemId} itemData={mi} />
                    ))}
                </div>
            ) : (
                <p>No menu items found</p>
            )}
        </div>
    );
}