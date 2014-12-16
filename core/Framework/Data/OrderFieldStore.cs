using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using LComplete.Framework.Common;

namespace LComplete.Framework.Data
{
    /// <summary>
    /// OrderField ���ϵķ�װ
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public class OrderFieldStore<TModel> where TModel : class
    {
        public IList<OrderField<TModel>> OrderFields { get; private set; }

        public OrderFieldStore()
        {
            OrderFields = new List<OrderField<TModel>>();
        }

        /// <summary>
        /// ���һ�������ֶ�
        /// </summary>
        /// <param name="fieldExpression"></param>
        /// <param name="orderType"></param>
        /// <param name="priority"></param>
        public void Add(Expression<Func<TModel, object>> fieldExpression, OrderType orderType = OrderType.None, int priority = 0)
        {
            //δ�ṩ���ȼ�ʱ���Զ��������ȼ�
            if (priority == 0)
                priority = OrderFields.Count == 0 ? 1 : OrderFields.Max(t => t.Priority) + 1;

            OrderFields.Add(new OrderField<TModel>(fieldExpression, orderType, priority));
        }

        /// <summary>
        /// ���һ�������ֶ�
        /// </summary>
        /// <param name="orderKey"></param>
        /// <param name="orderType"></param>
        /// <param name="priority"></param>
        public void Add(string orderKey, OrderType orderType = OrderType.None, int priority = 0)
        {
            priority = priority > 0 ? priority : (OrderFields.Max(t => t.Priority) + 1);
            OrderFields.Add(new OrderField<TModel>(orderKey, orderType, priority));
        }

        /// <summary>
        /// ��ȡ�����ֶ�
        /// </summary>
        /// <param name="filedExpression"></param>
        /// <returns></returns>
        public OrderField<TModel> GetOrderField(Expression<Func<TModel, object>> filedExpression)
        {
            string orderKey = ExpressionUtils.ParseMemberName(filedExpression);
            return GetOrderField(orderKey);
        }

        /// <summary>
        /// ��ȡ�����ֶ�
        /// </summary>
        /// <param name="orderKey"></param>
        /// <returns></returns>
        public OrderField<TModel> GetOrderField(string orderKey)
        {
            OrderField<TModel> orderField = OrderFields.FirstOrDefault(t => t.OrderKey == orderKey);
            return orderField;
        }

        /// <summary>
        /// ��ȡ�����ȼ�������������ֶ�
        /// </summary>
        /// <returns></returns>
        public IList<OrderField<TModel>> GetOrderFieldsByPriority(bool removeNoneOrder = true)
        {
            if (removeNoneOrder)
                return OrderFields.OrderBy(t => t.Priority).Where(t => t.OrderType != OrderType.None).ToList();

            return OrderFields.OrderBy(t => t.Priority).ToList();
        }

        /// <summary>
        /// ���������ֶ�
        /// </summary>
        /// <param name="fieldExpression"></param>
        /// <param name="orderType"></param>
        /// <param name="priority"></param>
        public void SetOrderField(Expression<Func<TModel, object>> fieldExpression, OrderType orderType, int priority = 0)
        {
            OrderField<TModel> orderField = GetOrderField(fieldExpression);
            if (orderField != null)
            {
                orderField.OrderType = orderType;
                if (priority > 0)
                    orderField.Priority = priority;
            }
            else
            {
                Add(fieldExpression, orderType, priority);
            }
        }

        /// <summary>
        /// ���������ֶ�
        /// </summary>
        /// <param name="orderKey"></param>
        /// <param name="orderType"></param>
        /// <param name="priority"></param>
        public void SetOrderField(string orderKey, OrderType orderType, int priority = 0)
        {
            OrderField<TModel> orderField = GetOrderField(orderKey);
            if (orderField != null)
            {
                orderField.OrderType = orderType;
                if (priority > 0)
                    orderField.Priority = priority;
            }
            else
            {
                Add(orderKey, orderType, priority);
            }
        }

        /// <summary>
        /// �����������ȼ�����������
        /// </summary>
        /// <param name="o">��.��ƴ�ӵ�����(���ֱ�ʾ����:��1��ʼ,-��ʾ����)</param>
        public void ChangeOrderFlags(string o)
        {
            IList<int> orderFlags = StringParseUtils.ParseJoinString(o, '.').Distinct().ToList();
            IList<int> changedIndexs = new List<int>();

            //�������ȼ��������ʶ
            int incrementPriority = 1;
            for (int i = 0; i < orderFlags.Count; i++)
            {
                int orderFlag = orderFlags[i];
                int fieldIndex = Math.Abs(orderFlag) - 1;//�ַ����е�������ʶ��1��ʼ
                if (fieldIndex < OrderFields.Count)
                {
                    OrderFields[fieldIndex].Priority = incrementPriority++;
                    OrderFields[fieldIndex].OrderType = orderFlag > 0 ? OrderType.Ascending : OrderType.Descending;
                    changedIndexs.Add(fieldIndex);
                }
            }

            //������������ ��δ�������ļ�¼�е�Ĭ������ȥ��
            if (changedIndexs.Count > 0)
            {
                for (int i = 0; i < OrderFields.Count; i++)
                {
                    if (!changedIndexs.Contains(i))
                    {
                        OrderFields[i].OrderType = OrderType.None;
                    }
                }
            }

            OrderFields = OrderFields.OrderBy(t => t.Priority).ToList();//���°����ȼ�����
        }

        /// <summary>
        /// ��ȡ�ַ�����ʽ��Ĭ�������ʶ
        /// </summary>
        /// <returns></returns>
        public string MakeOrderFlags()
        {
            return MakeOrderFlags(string.Empty, OrderType.None);
        }

        /// <summary>
        /// ��ȡ�ַ�����ʽ�������ʶ
        /// </summary>
        /// <param name="fieldExpression">Ҫ���ĵ������ֶεı��ʽ</param>
        /// <param name="orderType">��������</param>
        /// <param name="isKeepOtherOrder">�����ֶ��Ƿ񱣳�ԭʼ����������</param>
        /// <returns>�ַ�����ʽ���������ȼ��ͱ�ʶ����Ҫ���ĵ������������ȼ����</returns>
        public string MakeOrderFlags(Expression<Func<TModel, object>> fieldExpression, OrderType orderType, bool isKeepOtherOrder = false)
        {
            string orderKey = ExpressionUtils.ParseMemberName(fieldExpression);
            return MakeOrderFlags(orderKey, orderType, isKeepOtherOrder);
        }

        /// <summary>
        /// ��ȡ�ַ�����ʽ�������ʶ
        /// </summary>
        /// <param name="orderKey">Ҫ���ĵ���������key</param>
        /// <param name="orderType">��������</param>
        /// <param name="isKeepOtherOrder">�����ֶ��Ƿ񱣳�ԭʼ����������</param>
        /// <returns>�ַ�����ʽ���������ȼ��ͱ�ʶ����Ҫ���ĵ������������ȼ����</returns>
        public string MakeOrderFlags(string orderKey, OrderType orderType, bool isKeepOtherOrder = false)
        {
            IList<int> orderFlags = new List<int>();
            int makeOrderFlag = 0;

            for (int i = 0; i < OrderFields.Count; i++)
            {
                OrderField<TModel> orderField = OrderFields[i];
                string currentOrderKey = orderField.OrderKey;

                if (currentOrderKey == orderKey)
                {
                    if (orderType != OrderType.None)
                        makeOrderFlag = orderField.RawPriorityIndex*(orderType == OrderType.Descending ? -1 : 1);
                }
                else
                {
                    OrderType otherOrderType = orderField.OrderType;
                    if (isKeepOtherOrder)
                        otherOrderType = orderField.RawOrderType;
                    if (otherOrderType != OrderType.None)
                        orderFlags.Add(orderField.RawPriorityIndex*(otherOrderType == OrderType.Descending ? -1 : 1));
                }
            }

            if (makeOrderFlag != 0)
                orderFlags.Insert(0, makeOrderFlag);

            return string.Join(".", orderFlags.Select(t => t.ToString()));
        }

    }
}