   M S B A M L   `   `   `  ����L  HTektaRevitPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullE'clr-namespace:TektaRevitPlugins.WindowsTektaRevitPlugins.Windows  5   	   -    'TektaRevitPlugins.Windows.EnumValuesWnd   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   9local'clr-namespace:TektaRevitPlugins.Windows     5   	     � Title$  Enumerate Values��5   	   $	��150��6"   $	��300��6/    � 
ResizeMode$ NoResize=�6;    � 	Resources 5	       �� 
TargetType �� Property �� Value��%G  '��       '�� G     '�� �     '�� �     �� 5
   
   " ��6   .�5      ��  � ��$ 7q�6'   5      �� 5   
   " ��6   .�5      ��  � ��$ 7q�6'   5      �� 5   
   " ��6   .�5      ��  � ��$ 7q�6'   5      �� 5   
   " ��6   .�5      ��  � ��$
 Bold"�6+   5      5      .��5      � #��Z�6    � RowDefinitions 5   
   �� 5      $��* �6   �� 5      $
��Auto �6   �� 5       $��* �6   �� 5!      $��* �6    � ColumnDefinitions 5$   
   �� 5%      $��* �6   �� 5&      $��* �6   �� 5'      $��* �6   .+�5*   
   �� $��0��6   $��0��6!   $��	Parameter��62   $��Center=�6C   $��Center=�6`   �� 5+   
   -   5         ��Name$ cb_parameter_names��5+      $��0��6/   $��1��6<   $��2��6M   	 �� 
IsReadOnly	 . 6a   �� 5-   
   $��1��6   $��0��6!   $��Prefix��61   $��Center=�6?   �� 5.   
   $��1��6   $��1��6!   $��Position��61   $��Center=�6A   �� 5/   
   $��1��6   $��2��6!   $��Suffix��61   $��Center=�6?   �� 51   
   -   5        
 ��Name$
 
txt_prefix��51      $��2��6&   $��0��63   �� 52   
   -   5        $
 txt_position��52      $��2��6(   $��1��65   �� 53   
   -   5        $
 
txt_suffix��53      $��2��6&   $��2��63   �� 55   
   -   5         ��Name$ btn_ok��55      $��3��6!   $��1��6.   $��OK��6>   6K   �� 56   
   -   5        $ 
btn_cancle��56      $��3��6%   $��2��62   $��Cancel��6B   6S   57      58      