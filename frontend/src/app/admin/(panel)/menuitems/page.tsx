import Link from "next/link";
import {cn} from "@/lib/utils";
import {buttonVariants} from "@/components/ui/button";
import AdminMenuItemsSearchBar from "@/components/admin/menuItems/MenuItemsSearchBar";

export default async function AdminMenuItemsPage(
    { searchParams }: { searchParams: Promise<{ param?: string }>})
{
    const { param } = await searchParams;
    return (
        <div className='flex flex-1 flex-col m-4'>
            <div className='flex justify-between items-center'>
                <h1 className='text-2xl'>Menu items</h1>
                <div className='flex justify-end items-center gap-2'>
                    <AdminMenuItemsSearchBar val={param} />
                    <Link
                        href='/admin/locations/add'
                        className={cn(
                            buttonVariants({ variant: 'default', size: 'default' }),
                        )}
                    >
                        Add new menu item
                    </Link>
                </div>
            </div>
            {/*<AdminLocationsMain locationData={param} />*/}
            {/*  Main here  */}
        </div>
    );
}