import Link from 'next/link';
import { ShoppingCart } from 'lucide-react';
import { cn } from '@/lib/utils';
import { buttonVariants } from '@/components/ui/button';

export default function AdminCartButton() {
    return (
        <Link
            href='/admin/cart'
            aria-label='Cart'
            className={cn(buttonVariants({ variant: 'default', size: 'icon' }))}
        >
            <ShoppingCart />
        </Link>
    );
}
