   M S B A M L   `   `   `  ����L  HTektaRevitPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null5clr-namespace:TektaRevitPluginsTektaRevitPlugins  5   	   ,    &TektaRevitPlugins.CreateSharedParamWin   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   1localclr-namespace:TektaRevitPlugins     5   	     � Title$  Create Shared Parameter��5   	   $	��300��6)    � SizeToContent$ Height=�5	   	    � 
ResizeMode$ NoResize=�6     � 	Resources 5
       �� 
TargetType �� Property �� Value��%`  '��       '�� x     '��     '�� q    '�� �    �� 5   
   " ��6   .�5      ��  � ��$ 2q�6'   �� 5       �@��SelectedIndex$ 0��6.   5      �� 5   
   " ��6   .�5      ��  � ��$ 75��6&   �� 5       � ��$ 25��6'   �� 5       � ��$ 2q�6'   5      �� 5   
   " ��6   .�5      ��  � ��$ 2q�6'   �� 5       � ��$ 2q�6(   5      �� 5   
   " ��6   .�5      ��  � ��$ 3q�6'   �� 5       �@d�IsThreeState .  6-   5      �� 5    
   " ��6   .�5!      ��  � ��$ 3q�6'   5"      5#      .��5%      � $��3q�6    � RowDefinitions 5&   
   �� 5'      $
��Auto �6   �� 5(      $
��Auto �6   .+�5+   
   �� $��Parameter Properties��6   $��5q�61   .��5,      � $��3q�6    � ColumnDefinitions 5-      �� 5.      $
��Auto �6'   �� 5/      $��* �6'    52      �� 53      $
��Auto �6$   �� 54      $
��Auto �6$   �� 55      $
��Auto �6$   �� 56      $
��Auto �6$   �� 57      $
��Auto �6$   �� 58      $
��Auto �6$   �� 59      $
��Auto �6$   �� 5:      $
��Auto �6$   �� 5;      $
��Auto �6$   �� 5<      $
��Auto �6$   .+�5?      �� $��0��6   $��0��6)   $��
File Path:��69   �� 5@      $��1��6   $��0��6)   $��Groups:��69   �� 5A      $��2��6   $��0��6)   $��Name:��69   �� 5B      $��3��6   $��0��6)   $��Type:��69   �� 5C      $��4��6   $��0��6)   $��Group:��69   �� 5D      $��5��6   $��0��6)   $��	Category:��69   �� 5E      $��6��6   $��0��6)   $��	Instance:��69   �� 5F      $��7��6   $��0��6)   $��Modifiable:��69   �� 5G      $��8��6   $��0��6)   $��Visible:��69   �� 5H      $��9��6   $��0��6)   $��
Group Var:��69   �� 5J      -   5        	 ��Name$	 tb_file_path��5J      $��0��60   $��1��6=   
 �� 
IsReadOnly
 . 6M   �� 5K      -   5         ��Name$ cb_def_group��5K      $��1��61   $��1��6>    �� 
IsEditable . 6N   �� 5L      -   5        $	 tb_param_name��5L      $��2��61   $��1��6>   �� 5M      -   5        $ cb_param_type��5M      $��3��62   $��1��6?   �� 5N      -   5        $ cb_param_group��5N      $��4��63   $��1��6@   �� 5O      -   5        $ cb_category��5O      $��5��60   $��1��6=   �� 5P      -   5         ��Name$ chb_is_instance��5P      $��6��64   $��1��6A   $��Yes/No��6Q   �� 5Q      -   5        $ chb_is_modifiable��5Q      $��7��66   $��1��6C   $��Yes/No��6S   �� 5R      -	   5        $ chb_is_visible��5R      $��8��63   $��1��6@   $��Yes/No��6P   �� 5S      -
   5        $ chb_is_vary_btw_groups��5S      $��9��6;   $��1��6H   $��Yes/No��6X   5T      5U      �� 5W   
   $��1��6    �� Orientation$ 
Horizontal=�6"   $��Right=�6;   .+�5X      �� -   5         ��Name$ btn_ok��5X      $	��_Ok��6%   63   �� 5Y      -   5        $ 
btn_cancel��5Y      $��_Cancel��6)   6;   5Z      5[      5\      