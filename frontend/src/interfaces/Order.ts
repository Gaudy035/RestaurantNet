import CartItem from '@/interfaces/CartItem';
import DeliveryData from '@/interfaces/DeliveryData';
import OrderStatus from '@/types/order-status';
import OrderType from '@/types/order-type';
import PaymentMethod from '@/types/payment-method';

interface Order {
    orderId: string;
    clientId: string | null;
    locationId: string;
    price: number;
    phoneNumber: string;
    orderTime: string;
    items: CartItem[];
    paymentMethod: PaymentMethod;
    isPaid: boolean;
    orderStatus: OrderStatus;
    orderType: OrderType;
    delivery: DeliveryData | null;
}

export default Order;
