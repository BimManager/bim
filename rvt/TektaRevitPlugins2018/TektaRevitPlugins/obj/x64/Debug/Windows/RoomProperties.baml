   M S B A M L   `   `   `  ����L  HTektaRevitPlugins, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null5clr-namespace:TektaRevitPluginsTektaRevitPlugins  5   	   &     TektaRevitPlugins.RoomProperties   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   1localclr-namespace:TektaRevitPlugins     5   	     � Title$  RoomProperties��5   	    � SizeToContent$ Height=�6    $	��300��67    � 	Resources 5	       �� 
TargetType �� Property �� Value��%S  '��       '�� j     '�� �     �� 5
   
   " ��6   .�5      ��  � ��$ 3q�6'   �� 5       � ��$ 3q�6(   5      �� 5   
   " ��6   .�5      ��  �@��SelectedIndex$ 0��6.   �� 5       � ��$ 5,2,5,2q�6'   5      �� 5   
   " ��6   .�5      ��  � ��$ 0,3,0,3q�6'   5      5      .��5      �  � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $��* �6   .+�5   
   �� -   5        $��
stackPanel��5      $��0��6'   � 5   
   -   5         �Name$ wrapper��5      $��1��6%    � Orientation$ 
Horizontal=�62   $	��5,0q�6K   �� 5   
   $��2��6   	 �� Orientation$	 
Horizontal=�6"   $��Right=�6;   .+�5      �� -   5        
 ��Name$
 ok_btn��5      $��OK��6%   62   �� 5       -   5        $
 
cancel_btn��5       $��Cancel��6)   $��3q�6S   5!      5"      5#      