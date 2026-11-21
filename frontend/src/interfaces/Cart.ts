import CartItem from '@/interfaces/CartItem';

interface Cart {
    items: CartItem[];
    totalPrice: number;
}

export default Cart;
